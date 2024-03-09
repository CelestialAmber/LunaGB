using System;
using LunaGB.Core.Debug;
using System.Threading;
using System.Diagnostics;
using LunaGB.Graphics;
using LunaGB.Utils;

namespace LunaGB.Core
{
	public class Emulator
	{

		CPU cpu;
		PPU ppu;
		Memory memory;
		ROM rom;
		Disassembler disassembler;
		public Display display;
		public Debug.Debugger debugger;

		public bool isRunning = false;
		public bool paused = false;
		public bool loadedRom => rom.loadedRom;
		public bool debug = false;
		public bool doReset;
		public bool pausedOnBreakpoint;
		CancellationToken ctoken;
		object emuStepLock = new object();

		public const int maxCycles = 4194304; //the original gb clock speed is 4.194 mhz
		public const float frameRate = 59.73f; //frame rate/refresh rate
		int cycles = 0;
		int frameCycleCount = 0;
		int divCycleTimer = 0;
		int timaCycleTimer = 0;
		int cyclesPerFrame = 0;
		double msPerFrame = 0d;
		byte prevJOYP;
		Logger logger;
		Stopwatch sw;

		public Emulator(Debug.Debugger debugger)
		{
			rom = new ROM();
			memory = new Memory(rom, debugger);
			display = new Display();
			cpu = new CPU(memory);
			ppu = new PPU(memory, display);
			disassembler = new Disassembler(memory);
			this.debugger = debugger;
			Input.memory = memory;
			logger = new Logger("log.txt");
			cyclesPerFrame = (int)Math.Floor((float)maxCycles/59.73f);
			msPerFrame = 1000d*(1d/frameRate);
			sw = new Stopwatch();
			//Setup events
			memory.OnLCDEnableChange += LCDEnableChangeCallback;
			memory.OnMemoryError += ErrorCallback;
			cpu.OnCPUError += ErrorCallback;
		}

		//Loads the specified ROM.
		public void LoadROM(string romPath) {
			rom.OpenROM(romPath);
		}

		//Starts the emulator.
		public void Start(CancellationToken token) {
			isRunning = true;
			frameCycleCount = 0;
			divCycleTimer = 0;
			timaCycleTimer = 0;
			paused = false;
			debugger.InitBreakpoints();
			memory.Init();
			cpu.Init();
			ppu.Init();
			rom.Init();
			ctoken = token;
			display.Clear();
			display.Render();
			prevJOYP = memory.GetIOReg(IORegister.P1);
			Run();
		}

		public void Run() {
			//Start the stopwatch
			sw.Start();

			while (isRunning && !ctoken.IsCancellationRequested) {
				/* If breakpoints are enabled and we're not manually stepping, check whether
				one of them would be hit by executing the next instruction */
				if (debugger.breakpointsEnabled)
				{
					debugger.OnExecute(cpu.pc);
				}

				//Wait until the emulator is unpaused or a manual step happens
				while (paused)
				{
					Thread.Sleep(100);
				}

				Step();
			}
		}

		public void DoSingleStep()
		{
			debugger.stepping = true;
			Console.WriteLine("Step");
			Step();
			debugger.stepping = false;
		}

		public void Step()
		{
			lock(emuStepLock){
				if (debugger.stepping) {
					PrintDebugInfo();
				}
				if(debug){
					LogDebugInfo();
				}

				//Check the JOYP register to see if any buttons were pressed. If so,
				//request a joypad interrupt, and exit stop mode if enabled.
				CheckJOYP();

				//Step the CPU
				cpu.Step();
				
				//Update the different cycle variables by the number of cycles the CPU took.
				int cyclesTaken = cpu.cycles;
				cycles += cyclesTaken;
				frameCycleCount += cyclesTaken;

				bool ppuActive = ppu.lcdcEnable == 1 ? true : false;

				//Step everything else by the number of cycles the CPU took.
				for(int i = 0; i < cyclesTaken; i++){
					//Step OAM DMA if currently doing an OAM DMA transfer
					if(memory.doingDMATransfer){
						memory.OAMDMAStep();
					}

					//Step the PPU if it's active
					if(ppuActive){
						ppu.Step();
					}

					//Update DIV and TIMA registers
					UpdateDIVAndTIMA();
				}

				//Check if we've reached the end of the frame in cycles
				if(frameCycleCount >= cyclesPerFrame){
					//Render the screen, and keep the thread waiting until we're at the end of frame in actual time.
					if(display.enabled) display.Render();
					frameCycleCount %= cyclesPerFrame;
					//TODO: is there a better way to keep a thread waiting than this?
					while(sw.Elapsed.TotalMilliseconds < msPerFrame){
						Thread.Sleep(0);
					}
					//Once the amount of time for a single frame has passed, restart the stopwatch for the next frame.
					sw.Restart();
				}

				if (cycles >= maxCycles){
					cycles %= maxCycles;
					frameCycleCount = cycles;
				}
			}
		}

		void LCDEnableChangeCallback(bool state){
			display.enabled = state;
			if(state == false){
				display.Clear();
				display.Render();
			}
		}

		//Called by the emulator if an error occurs somewhere (CPU, memory, etc...)
		void ErrorCallback(){
			Stop();
			PrintDebugInfo();
		}

		//Cycle numbers for how often to increment TIMA based on the selected
		//frequency in TAC.
		int[] tacClockIncrementFrequencyCycles = new int[]{256,4,16,64};

		void UpdateDIVAndTIMA(){
			divCycleTimer++;
			timaCycleTimer++;

			//Check if DIV should be incremented
			if(divCycleTimer >= 16384){
				divCycleTimer %= 16384;
				memory.hram[(int)IORegister.DIV]++;
			}

			//Check if TIMA should be incremented
			byte tac = memory.GetIOReg(IORegister.TAC);
			byte tma = memory.GetIOReg(IORegister.TMA);
			byte tima = memory.GetIOReg(IORegister.TIMA);

			int timerEnableFlag = (tac >> 2) & 1;
			int timerClockSelect = tac & 0x3;

			if(timerEnableFlag == 1){
				int cyclesPerIncrement = tacClockIncrementFrequencyCycles[timerClockSelect];
				while(timaCycleTimer >= cyclesPerIncrement){
					timaCycleTimer -= cyclesPerIncrement;
					tima++;
					//If tima overflows, set it to tma, and request a timer interrupt
					if(tima == 0){
						tima = tma;
						//TODO: For some reason, enabling this breaks Dr. Mario. Why?
						memory.SetHRAMBit((int)IORegister.IF, 2, 1);
					}
				}
				memory.hram[(int)IORegister.TIMA] = tima;
			}
		}

		//Stops the emulator.
		public void Stop() {
			isRunning = false;
		}

		void CheckJOYP(){
			byte JOYP = memory.GetIOReg(IORegister.P1);

			//Check if any of bits 0-3 of JOYP register went from high to low
			for(int i = 0; i < 4; i++){
				if(((prevJOYP >> i) & 1) == 1 && ((JOYP >> i) & 1) == 0){
					//If so, request a joypad interrupt
					memory.SetHRAMBit((int)IORegister.IF, 4, 1);
					//If stop mode is active, disable it
					if(cpu.stopMode == true){
						cpu.stopMode = false;
					}
					break;
				}
			}

			prevJOYP = JOYP;
		}

		public void PrintDebugInfo()
		{
			Console.WriteLine(disassembler.Disassemble(cpu.pc));
			Console.WriteLine(cpu.GetCPUStateInfo());
			Console.WriteLine(ppu.GetPPUStateInfo());
			Console.WriteLine("Cycles: " + cycles);
			Console.WriteLine();
		}

		public void LogDebugInfo(){
			logger.Log(disassembler.Disassemble(cpu.pc));
			logger.Log(cpu.GetCPUStateInfo());
			logger.Log(ppu.GetPPUStateInfo());
			logger.Log("Cycles: " + cycles);
			logger.Log("");
		}

		public Memory GetMemory(){
			return memory;
		}

		public LunaImage GetScreenBitmap(){
			return display.display;
		}

	}
}

