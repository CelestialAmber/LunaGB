using System;
using System.Threading;
using System.Diagnostics;
using Tsukimi.Graphics;
using Tsukimi.Utils;
using System.Text;
using LunaGB;
using LunaGB.ROMMappers;

namespace Tsukimi.Core {
	public class LunaGBEmulator : Emulator {
		CPU cpu;
		PPU ppu;
		Memory memory;
		ROM rom;
		Serial serial;
		Disassembler disassembler;
		public Debug.Debugger debugger;

		public new const string name = "LunaGB";
		public new const EmulatorSystem system = EmulatorSystem.GB;
		public new const int screenWidth = 160;
		public new const int screenHeight = 144;
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
		public System.Timers.Timer updateSaveFileTimer;
		GBSystem emulatedSystem;
		bool onGBC;

		int totalFrames;
		public float averageFrameTime;

		public LunaGBEmulator(Debug.Debugger debugger)
		{
			rom = new ROM();
			memory = new Memory(rom, debugger);
			display = new Display(screenWidth, screenHeight);
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

		public override bool LoadedRom()
		{
			return rom.loadedRom;
		}

        //Loads the specified ROM.
        public override void LoadFile(string romPath) {
			rom.OpenROM(romPath);
		}

		//Starts the emulator.
		public override void Start(CancellationToken token) {
			ctoken = token;
			isRunning = true;
			divCycleTimer = 0;
			timaCycleTimer = 0;
			paused = false;
			finishedFrame = false;

			totalFrames = 0;
			averageFrameTime = 0;

			UpdatePreviousJoypadFlags();
			DetermineSystem();
			//Check if we're running on GBC
			onGBC = emulatedSystem == GBSystem.CGB;

			//Init all of the components
			debugger.InitBreakpoints();
			memory.Init(onGBC);
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

		//Determines which system should be emulated.
		public void DetermineSystem(){
			//If auto detect is enabled, automatically determine which system should be used.
			if(Options.autoDetectSystem){
				if(rom.isGBCCompatible || rom.isGBCOnly) Options.system = GBSystem.CGB;
				else Options.system = GBSystem.DMG;
			}

			//If the emulated system is CGB but the game is GB only, emulate the GBC in GB compatability mode
			if(Options.system == GBSystem.CGB && rom.isGBOnly){
				Options.system = GBSystem.CGB_DMG;
			}
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

				//Check if a frame was finished
				if (finishedFrame)
				{
					//If so, if frame limiting is enabled, sleep until the time per frame has passed.
					if (Options.limitFrameRate) Sleep();

					//Update the FPS counter, and reset the stopwatch
					UpdateFPS();
					sw.Restart();
					finishedFrame = false;
				}
			}
		}

		//TODO: is there a better way to keep a thread waiting than this?
		void Sleep()
		{
			while (sw.Elapsed.TotalMilliseconds < msPerFrame)
			{
			}
		}

		public override void DoSingleStep()
		{
			debugger.stepping = true;
			Console.WriteLine("Step");
			Step();
			if (finishedFrame) finishedFrame = false;
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
						//Render the screen, and notify the emulator a frame was finished.
						if(display.enabled) display.Render();
						finishedFrame = true;
					}
				}

				if (cycles >= maxCycles){
					cycles %= maxCycles;
				}
			}
		}

		void UpdateFPS(){
			float actualFrameTimeMs = (float)sw.Elapsed.TotalMilliseconds;

			//Calculate average fps
			averageFrameTime = ((averageFrameTime * totalFrames) + actualFrameTimeMs) / (totalFrames + 1);
			totalFrames++;

			//Reset frame count about every second
			if (totalFrames >= 60)
			{
				totalFrames = 0;
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
		int[] tacClockIncrementFrequencyCycles = new int[]{1024,16,64,256};

		void UpdateDIVAndTIMA(){
			if(!cpu.stopMode)divCycleTimer++;

			//Check if DIV should be incremented
			if(divCycleTimer == 256){
				divCycleTimer = 0;
				memory.regs.DIV++;
			}

			//Check if TIMA should be incremented
			byte tma = memory.regs.TMA;
			byte tima = memory.regs.TIMA;

			int timerEnableFlag = memory.regs.TAC_EN;
			int timerClockSelect = memory.regs.TAC_CLK;

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
		public override void TogglePause(){
			paused = !paused;
			//Pause/unpause the rtc timer if the cartridge has one.
			if(rom.romMapper.hasTimer){
				rom.romMapper.ToggleTimer(paused);
			}
		}

		//Stops the emulator.
		public override void Stop() {
			isRunning = false;
			//If the ROM uses save data, save it before stopping the emulator.
			if(rom.useSaveFile){
				updateSaveFileTimer.Stop(); //Stop the save file timer
				rom.SaveSRAMToSaveFile();
			}
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

		public override LunaImage GetScreenBitmap(){
			return display.display;
		}

	}
}

