using System;
using LunaGB.Core.Debug;
using System.Threading;
using LunaGB.Graphics;
using System.Numerics;

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
		public bool debug = true;
		public bool doReset;
		public bool pausedOnBreakpoint;
		public delegate void FinishRenderingEvent();
		public event FinishRenderingEvent OnFinishRendering;
		CancellationToken ctoken;
		object emuStepLock = new object();

		public const int maxCycles = 4194304; //the original gb clock speed is 4.194 mhz
		public const float frameRate = 59.73f; //frame rate/refresh rate
		int frameCycleCount = 0;
		int cyclesPerFrame = 0;


		public Emulator(Debugger debugger)
		{
			rom = new ROM();
			memory = new Memory(rom, debugger);
			cpu = new CPU(memory);
			ppu = new PPU(memory);
			disassembler = new Disassembler(memory);
			this.debugger = debugger;
			cyclesPerFrame = (int)Math.Floor((float)maxCycles/frameRate);
		}

		//Loads the specified ROM.
		public void LoadROM(string romPath) {
			rom.OpenROM(romPath);
		}

		//Starts the emulator.
		public void Start(CancellationToken token) {
			isRunning = true;
			frameCycleCount = 0;
			paused = false;
			debugger.InitBreakpoints();
			memory.Init();
			cpu.Init();
			ppu.Init();
			ctoken = token;
			ClearScreen();
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
			Step();
			debugger.stepping = false;
		}

		public void Step()
		{
			lock(emuStepLock){
				if (debug && debugger.stepping) {
					Console.WriteLine("Step");
					RenderFrame();
					PrintDebugInfo();
				}

				int prevCycles = cpu.cycles;

				cpu.ExecuteInstruction();

				//If an error occured, stop the emulator.
				if (cpu.errorOccured == true || memory.writeErrorOccured == true)
				{
					Stop();
					Console.WriteLine(cpu.GetCPUStateInfo());
					return;
				}

				//Handle interrupts
				cpu.HandleInterrupts();
				
				int cyclesTaken = cpu.cycles - prevCycles;
				frameCycleCount += cyclesTaken;

				CheckSCRegister();

				ppu.Step(cyclesTaken);

				if(frameCycleCount > cyclesPerFrame){
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

		void RenderFrame(){
			//Render the background for testing
			ppu.DrawEntireBackground();
			OnFinishRendering?.Invoke();
		}


		//Stops the emulator.
		public void Stop() {
			isRunning = false;
		}

		//Code for blargg tests

		public string testRomString = "";
		bool readCharacter = false;

		public void CheckSCRegister() {
			byte scReg = memory.GetIOReg(IORegister.SC);

			if(scReg == 0x81 && !readCharacter) {
				readCharacter = true;
				byte sbReg = memory.GetIOReg(IORegister.SB);
				testRomString += (char)sbReg;
			}else if(readCharacter && scReg != 0x81) {
				readCharacter = false;
				Console.WriteLine(testRomString);
			}
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

