using System;
using System.Threading;
using System.Diagnostics;
using LunaGB.Graphics;
using LunaGB.Utils;
using System.Text;
using LunaGB.Core.ROMMappers;

namespace LunaGB.Core{
	public class Emulator{
		CPU cpu;
		PPU ppu;
		Memory memory;
		ROM rom;
		Serial serial;
		Disassembler disassembler;
		public Display display;
		public Debug.Debugger debugger;

		public bool isRunning = false;
		public bool paused = false;
		public bool loadedRom => rom.loadedRom;
		public bool debug = false;
		CancellationToken ctoken;
		object emuStepLock = new object();

		public const int maxCycles = 4194304; //the original gb clock speed is 4.194 mhz
		int cycles = 0;
		int divCycleTimer = 0;
		int timaCycleTimer = 0;
		double msPerFrame = 0d;
		int prevP10, prevP11, prevP12, prevP13;
		int lastLCDEnable;
		Logger logger;
		Stopwatch sw;
		public float currentFPS;
		public double frameTime;
		public System.Timers.Timer updateSaveFileTimer;
		GBSystem emulatedSystem;
		bool onGBC;

		public Emulator(Debug.Debugger debugger)
		{
			rom = new ROM();
			memory = new Memory(rom, debugger);
			display = new Display();
			cpu = new CPU(memory);
			ppu = new PPU(memory, display);
			serial = new Serial(memory);
			disassembler = new Disassembler(memory);
			this.debugger = debugger;
			Input.memory = memory;
			logger = new Logger("log.txt");
			msPerFrame = 1000d*(1d/Options.frameRate);
			sw = new Stopwatch();
			updateSaveFileTimer = new System.Timers.Timer(500);
			updateSaveFileTimer.Elapsed += OnUpdateSaveFileTimerElapsed;
			updateSaveFileTimer.AutoReset = true;
			//TODO: if auto detect system is enabled, use the detected system instead
			emulatedSystem = Options.system;

			//Setup events
			memory.OnLCDEnableChange += LCDEnableChangeCallback;
			memory.OnMemoryError += ErrorCallback;
			memory.OnSerialTransferEnable += serial.RequestTransfer;
			cpu.OnCPUError += ErrorCallback;

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
			divCycleTimer = 0;
			timaCycleTimer = 0;
			paused = false;
			UpdatePreviousJoypadFlags();

			//Check if we're running on GBC
			onGBC = emulatedSystem == GBSystem.CGB;

			//Init all of the components
			debugger.InitBreakpoints();
			memory.Init();
			cpu.Init(onGBC);
			ppu.Init(onGBC);
			rom.Init();

			lastLCDEnable = memory.regs.lcdcEnable;

			//Clear the display
			display.Clear();
			display.Render();

			//Start the timer for updating the rom's save file periodically if the rom uses save data.
			if(rom.useSaveFile){
				updateSaveFileTimer.Start();
			}
			
			//If the rom uses an rtc clock, start it
			if(rom.romMapper.hasTimer){
				MBC3 mbc3 = (MBC3)rom.romMapper;
				mbc3.rtc.Start();
			}

			if(Options.bootToPause){
				paused = true;
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
				//If double speed is active, divide the number by 2.
				int cyclesTaken = cpu.gbcCpuSpeed ? cpu.cycles/2 : cpu.cycles;
				cycles += cyclesTaken;

				
				int lcdcEnable = memory.regs.lcdcEnable;
				//Check if the LCD was reenabled
				//TODO: when the ppu is reenabled, for the first frame it should be put into
				//"mode 0" (a broken version of mode 2) that's 4 dots shorter, and OAM isn't
				//locked. This only applies for the first scanline, so maybe start from there?
				//Instead of that for now, I can just set it to mode 2 probably
				if(lastLCDEnable == 0 && lcdcEnable == 1){
					ppu.lcdReenabled = true;
				}
				lastLCDEnable = lcdcEnable;

				//Step everything else by the number of cycles the CPU took.
				for(int i = 0; i < cyclesTaken; i++){
					//If double speed is active, run the update functions twice
					//to compensate.
					for(int j = 0; j < (cpu.gbcCpuSpeed ? 2 : 1); j++){
						//Step OAM DMA if currently doing an OAM DMA transfer
						if(memory.doingDMATransfer){
							memory.OAMDMAStep();
						}

						//Update DIV and TIMA registers
						UpdateDIVAndTIMA();

						//Step serial
						serial.Step();
					}

					//Step the PPU if it's active
					if(lcdcEnable == 1){
						ppu.Step();
					}

					//Check if we've reached the end of the frame
					if(ppu.finishedFrame){
						ppu.finishedFrame = false;
						CheckJOYP();
						//Render the screen, and keep the thread waiting until we're at the end of frame in actual time.
						if(display.enabled) display.Render();
						frameTime = sw.Elapsed.TotalMilliseconds;
						if(Options.limitFrameRate){
							//TODO: is there a better way to keep a thread waiting than this?
							if(frameTime < msPerFrame){
								Thread.Sleep((int)(msPerFrame - frameTime));
							}
							while(sw.Elapsed.TotalMilliseconds < msPerFrame){
								Thread.Sleep(0);
							}
						}
						UpdateFPS();
						//Once the amount of time for a single frame has passed, restart the stopwatch for the next frame.
						sw.Restart();
					}
				}

				if (cycles >= maxCycles){
					cycles %= maxCycles;
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
			if(!cpu.stopMode)divCycleTimer++;

			//Check if DIV should be incremented
			if(divCycleTimer == 256){
				divCycleTimer = 0;
				memory.regs.DIV++;
			}

			//Check if TIMA should be incremented
			byte tac = memory.regs.TAC;
			byte tma = memory.regs.TMA;
			byte tima = memory.regs.TIMA;

			int timerEnableFlag = (tac >> 2) & 1;
			int timerClockSelect = tac & 0x3;

			if(timerEnableFlag == 1){
				timaCycleTimer++;
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
				memory.regs.TIMA = tima;
			}
		}

		//Pauses/Unpauses the emulator.
		public void TogglePause(){
			paused = !paused;
			//Pause/unpause the rtc timer if the cartridge has one.
			if(rom.romMapper.hasTimer){
				rom.romMapper.ToggleTimer(paused);
			}
		}

		//Stops the emulator.
		public void Stop() {
			isRunning = false;
			//Stop the update save file timer if it was enabled.
			updateSaveFileTimer.Stop();
			//Stop the rtc timer if the cartridge has one.
			if(rom.romMapper.hasTimer){
				MBC3 mbc3 = (MBC3)rom.romMapper;
				mbc3.rtc.Stop();
			}
		}

		void CheckJOYP(){
			int P10 = memory.regs.P10;
			int P11 = memory.regs.P11;
			int P12 = memory.regs.P12;
			int P13 = memory.regs.P13;

			//Check if any of bits 0-3 of JOYP register went from high to low
			if((prevP10 == 1 && P10 == 0) || (prevP11 == 1 && P11 == 0) || (prevP12 == 1 && P12 == 0)
			|| (prevP13 == 1 && P13 == 0)){
				//If so, request a joypad interrupt
				memory.RequestInterrupt(Interrupt.Joypad);
				//If stop mode is active, disable it
				if(cpu.stopMode == true){
					cpu.stopMode = false;
				}
			}

			UpdatePreviousJoypadFlags();
		}

		void UpdatePreviousJoypadFlags(){
			prevP10 = memory.regs.P10;
			prevP11 = memory.regs.P11;
			prevP12 = memory.regs.P12;
			prevP13 = memory.regs.P13;
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
			byte div = memory.regs.DIV;
			byte tma = memory.regs.TMA;
			byte tima = memory.regs.TIMA;
			byte IF = memory.regs.IF;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Format("AF = {0:X4}, BC = {1:X4}, DE = {2:X4}, HL = {3:X4}",cpu.AF, cpu.BC, cpu.DE, cpu.HL));
			sb.AppendLine(string.Format("PC = {0:X4}, SP = {1:X4}, IE = {2:X2}, IF = {3:X2}, IME = {4}",cpu.pc, cpu.sp, cpu.IE, IF, cpu.ime));
			//sb.AppendLine(string.Format("Flags (F): N = {0} Z = {1} C = {2} H = {3}", cpu.flagN, cpu.flagZ, cpu.flagC, cpu.flagH));
			//sb.AppendLine(string.Format("LY = {0:X2}", ppu.ly));
			//sb.AppendLine(string.Format("DIV = {0:X2}, TMA = {1:X2}, TIMA = {2:X2}", div, tma, tima));
			//sb.AppendLine("Cycles: " + cycles + "\n");
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

