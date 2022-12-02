using System;
using LunaGB.Core.Debug;

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
		public bool debug = true;

		public const int maxCycles = 4194304; //the original gb clock speed is 4.194 mhz



		public Emulator()
		{
			ppu = new PPU();
			display = new Display();
            rom = new ROM();
            memory = new Memory(rom);
            cpu = new CPU(memory);
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
			Run(token);
        }

		public void Run(CancellationToken token) {
			while (isRunning && !token.IsCancellationRequested) {
				if (paused) {
					Thread.Sleep(1000);
					continue;
				}

				Step();

				if (cpu.errorOccured == true) break;
			}
        }

		public void DoSingleStep()
		{
			if(!debug) PrintDebugInfo();
			Debugger.stepping = true;
			Step();
			Debugger.stepping = false;
		}

		public void Step()
		{
			if (debug){
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

			if (cpu.cycles >= maxCycles) cpu.cycles = 0;
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

