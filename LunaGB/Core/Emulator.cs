using System;
using System.Threading;
using System.Diagnostics;
using LunaGB.Graphics;
using LunaGB.Utils;
using System.Text;

namespace LunaGB.Core{
	public class Emulator{
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
		public const int cyclesPerFrame = 70224; //~= 4194304/59.7275
		int cycles = 0;
		int frameCycleCount = 0;
		int divCycleTimer = 0;
		int timaCycleTimer = 0;
		double msPerFrame = 0d;
		byte prevJOYP;
		Logger logger;
		Stopwatch sw;
		public float currentFPS;
		public System.Timers.Timer updateSaveFileTimer;

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
			msPerFrame = 1000d*(1d/Options.frameRate);
			sw = new Stopwatch();
			InitUpdateSaveFileTimer();
			//Setup events
			memory.OnLCDEnableChange += LCDEnableChangeCallback;
			memory.OnMemoryError += ErrorCallback;
			cpu.OnCPUError += ErrorCallback;

		}

		void InitUpdateSaveFileTimer(){
			updateSaveFileTimer = new System.Timers.Timer(500);
			updateSaveFileTimer.Elapsed += OnUpdateSaveFileTimerElapsed;
			updateSaveFileTimer.AutoReset = true;
		}

		private void OnUpdateSaveFileTimerElapsed(object? source, System.Timers.ElapsedEventArgs e){
			rom.UpdateSaveFile();
		}

		//Loads the specified ROM.
		public void LoadROM(string romPath) {
			rom.OpenROM(romPath);
		}

		//Starts the emulator.
		public void Start(CancellationToken token) {
			ctoken = token;
			isRunning = true;
			frameCycleCount = 0;
			divCycleTimer = 0;
			timaCycleTimer = 0;
			paused = false;
			prevJOYP = memory.GetIOReg(IORegister.P1);

			//Init all of the components
			debugger.InitBreakpoints();
			memory.Init();
			cpu.Init();
			ppu.Init();
			rom.Init();

			//Clear the display
			display.Clear();
			display.Render();

			//Start the timer for updating the rom's save file periodically if the rom uses save data.
			if(rom.useSaveFile){
				updateSaveFileTimer.Start();
			}

			//Start the emulator
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
				//CheckJOYP();

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
					CheckJOYP();
					//Render the screen, and keep the thread waiting until we're at the end of frame in actual time.
					if(display.enabled) display.Render();
					frameCycleCount %= cyclesPerFrame;
					if(Options.limitFrameRate){
						//TODO: is there a better way to keep a thread waiting than this?
						while(sw.Elapsed.TotalMilliseconds < msPerFrame){
							Thread.Sleep(0);
						}
					}
					UpdateFPS();
					//Once the amount of time for a single frame has passed, restart the stopwatch for the next frame.
					sw.Restart();
				}

				if (cycles >= maxCycles){
					cycles %= maxCycles;
					frameCycleCount = cycles;
				}
			}
		}

		//TODO: calculate fps over multiple frames
		void UpdateFPS(){
			currentFPS = (float)(1000f/sw.Elapsed.TotalMilliseconds);
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
		int[] tacClockIncrementFrequencyCycles = new int[]{1024,16,64,256};

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
						memory.RequestInterrupt(Interrupt.Timer);
					}
				}
				memory.hram[(int)IORegister.TIMA] = tima;
			}
		}

		//Stops the emulator.
		public void Stop() {
			isRunning = false;
			//Stop the update save file timer if it was enabled.
			updateSaveFileTimer.Stop();
		}

		void CheckJOYP(){
			byte JOYP = memory.GetIOReg(IORegister.P1);

			//Check if any of bits 0-3 of JOYP register went from high to low
			for(int i = 0; i < 4; i++){
				if(((prevJOYP >> i) & 1) == 1 && ((JOYP >> i) & 1) == 0){
					//If so, request a joypad interrupt
					memory.RequestInterrupt(Interrupt.Joypad);
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
			Console.WriteLine(GetEmulatorStateInfo());
			Console.WriteLine();
		}

		public void LogDebugInfo(){
			logger.Log(disassembler.Disassemble(cpu.pc));
			logger.Log(GetEmulatorStateInfo());
			logger.Log("");
		}

		string GetEmulatorStateInfo(){
			byte div = memory.GetIOReg(IORegister.DIV);
			byte tma = memory.GetIOReg(IORegister.TMA);
			byte tima = memory.GetIOReg(IORegister.TIMA);
			byte IF = memory.GetIOReg(IORegister.IF);

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Format("AF = {0:X4}, BC = {1:X4}, DE = {2:X4}, HL = {3:X4}",cpu.AF, cpu.BC, cpu.DE, cpu.HL));
			sb.AppendLine(string.Format("PC = {0:X4}, SP = {1:X4}, IE = {2:X2}, IF = {3:X2}, IME = {4}",cpu.pc, cpu.sp, cpu.IE, IF, cpu.ime));
			sb.AppendLine(string.Format("Flags (F): N = {0} Z = {1} C = {2} H = {3}", cpu.flagN, cpu.flagZ, cpu.flagC, cpu.flagH));
			sb.AppendLine(string.Format("LY = {0:X2}", ppu.ly));
			sb.AppendLine(string.Format("DIV = {0:X2}, TMA = {1:X2}, TIMA = {2:X2}", div, tma, tima));
			sb.AppendLine("Cycles: " + cycles + "\n");
			return sb.ToString();
		}

		public Memory GetMemory(){
			return memory;
		}

		public LunaImage GetScreenBitmap(){
			return display.display;
		}

	}
}

