using System;
using LunaGB.Core.Debug;
using System.Threading;

namespace LunaGB.Core
{
	public class Emulator
	{

		CPU cpu;
		PPU ppu;
		Display display;
		Memory memory;
		ROM rom;
		Disassembler disassembler;

		public bool isRunning = false;
		public bool paused = false;
		public bool loadedRom => rom.loadedRom;
		public bool debug = false;
		public bool doReset;
		public bool pausedOnBreakpoint;
		CancellationToken ctoken;
		object emuStepLock = new object();

		public const int maxCycles = 4194304; //the original gb clock speed is 4.194 mhz



		public Emulator()
		{
			display = new Display();
            rom = new ROM();
            memory = new Memory(rom);
            cpu = new CPU(memory);
			ppu = new PPU(memory);
			disassembler = new Disassembler(memory);
        }

		//Loads the specified ROM.
		public void LoadROM(string romPath) {
			rom.OpenROM(romPath);
        }

		//Starts the emulator.
		public void Start(CancellationToken token) {
			isRunning = true;
			paused = false;
			Debugger.InitBreakpoints();
			memory.Init();
			cpu.Init();
			ctoken = token;
			Run();
        }

		public void Run() {
			while (isRunning && !ctoken.IsCancellationRequested) {
				/* If breakpoints are enabled and we're not manually stepping, check whether
				one of them would be hit by executing the next instruction */
				if (Debugger.breakpointsEnabled)
				{
					Debugger.OnExecute(cpu.pc);
				}

				//Wait until the emulator is unpaused or a manual step happens
				while (paused)
				{
					Thread.Sleep(100);
				}

				Step();

				if (cpu.errorOccured == true) break;
			}
        }

		public void DoSingleStep()
		{
			Debugger.stepping = true;
			Step();
			Debugger.stepping = false;
		}

		public void Step()
		{
			lock(emuStepLock){
				Console.WriteLine("Step");
				if (debug) {
					PrintDebugInfo();
				}

				cpu.ExecuteInstruction();

				//If an error occured within the CPU, stop the emulator.
				if (cpu.errorOccured == true)
				{
					Stop();
					Console.WriteLine(cpu.GetCPUStateInfo());
					return;
				}

				CheckSCRegister();
				if (cpu.cycles >= maxCycles) cpu.cycles = 0;
			}
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

    }
}

