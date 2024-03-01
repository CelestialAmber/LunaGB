using System;
using LunaGB.Core.Debug;
using System.Threading;
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
		public Debugger debugger;

		public bool isRunning = false;
		public bool paused = false;
		public bool loadedRom => rom.loadedRom;
		public bool debug = false;
		public bool doReset;
		public bool pausedOnBreakpoint;
		public delegate void FinishRenderingEvent();
		public event FinishRenderingEvent OnFinishRendering;
		CancellationToken ctoken;
		object emuStepLock = new object();

		public const int maxCycles = 4194304; //the original gb clock speed is 4.194 mhz
		public const float frameRate = 59.73f; //frame rate/refresh rate
		int frameCycleCount = 0;
		int divCycleTimer = 0;
		int timaCycleTimer = 0;
		int cyclesPerFrame = 0;
		byte prevJOYP;
		Logger logger;


		public Emulator(Debugger debugger)
		{
			rom = new ROM();
			memory = new Memory(rom, debugger);
			cpu = new CPU(memory);
			ppu = new PPU(memory);
			disassembler = new Disassembler(memory);
			this.debugger = debugger;
			cyclesPerFrame = (int)Math.Floor((float)maxCycles/frameRate);
			Input.memory = memory;
			logger = new Logger("log.txt");
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
			ctoken = token;
			ClearScreen();
			prevJOYP = memory.GetIOReg(IORegister.P1);
			Run();
		}

		public void Run() {
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
				if (debug || debugger.stepping) {
					if(debugger.stepping) RenderFrame();
					PrintDebugInfo();
				}

				int prevCycles = cpu.cycles;

				//TODO: handle inputs and exiting stop mode
				/*
				STOP is terminated by one of the P10 to P13 lines going low.
				For this reason, d-pad and/or button inputs should be enabled by writing
				$00, $10 or $20 to the P1 register before entering STOP (depending on which
				buttons you want to terminate the STOP on).
				*/

				//cpu.stopMode = false;

				//Handle interrupts
				cpu.HandleInterrupts();

				cpu.ExecuteInstruction();

				//If an error occured, stop the emulator.
				if (cpu.errorOccured == true || memory.writeErrorOccured == true)
				{
					Stop();
					Console.WriteLine(cpu.GetCPUStateInfo());
					return;
				}

				//Check the JOYP register to see if any buttons were pressed. If so,
				//request a joypad interrupt.
				CheckJOYP();
				
				int cyclesTaken = cpu.cycles - prevCycles;
				frameCycleCount += cyclesTaken;
				divCycleTimer += cyclesTaken;
				timaCycleTimer += cyclesTaken;

				//Update DIV and TIMA registers
				UpdateDIVAndTIMA();

				ppu.Step(cyclesTaken);

				if(frameCycleCount >= cyclesPerFrame){
					//If we're at the end of a frame, render the screen
					RenderFrame();
					frameCycleCount %= cyclesPerFrame;
					Thread.Sleep(16);
				}

				if (cpu.cycles >= maxCycles){
					cpu.cycles = 0;
					frameCycleCount = 0;
				}
			}
		}

		//Cycle numbers for how often to increment TIMA based on the selected
		//frequency in TAC.
		int[] tacClockIncrementFrequencyCycles = new int[]{256,4,16,64};

		void UpdateDIVAndTIMA(){
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
						//memory.SetHRAMBit(0xFF00 + (int)IORegister.IF, 2, 1);
					}
				}
				memory.hram[(int)IORegister.TIMA] = tima;
			}
		}

		void RenderFrame(){
			//Render the background for testing
			ppu.DrawEntireBackground();
			OnFinishRendering?.Invoke();
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
					memory.SetHRAMBit(0xFF00 + (int)IORegister.IF, 4, 1);
					break;
				}
			}

			prevJOYP = JOYP;
		}

		public void PrintDebugInfo()
		{
			Console.WriteLine(disassembler.Disassemble(cpu.pc));
			Console.WriteLine(cpu.GetCPUStateInfo());
			Console.WriteLine();
		}

		public Memory GetMemory(){
			return memory;
		}

		public void ClearScreen(){

			for(int x = 0; x < 160; x++){
				for(int y = 0; y < 144; y++){
					ppu.display.SetPixel(x,y,Color.white);
				}
			}

			OnFinishRendering?.Invoke();
		}

		public LunaImage GetScreenBitmap(){
			return ppu.display;
		}

	}
}

