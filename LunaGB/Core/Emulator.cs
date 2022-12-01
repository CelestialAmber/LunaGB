using System;

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
		public void Start() {
			isRunning = true;
			memory.Init();
			cpu.Init();
			Run();
        }

        public void Run() {
            while (isRunning) {
				while(cpu.cycles < maxCycles) {
					if (debug){
						Console.WriteLine(disassembler.Disassemble(cpu.pc));
						Console.WriteLine(cpu.GetCPUStateInfo());
						Console.WriteLine();
					}
					cpu.ExecuteInstruction();
					//If an error occured within the CPU, stop the emulator.
					if(cpu.errorOccured == true) {
						isRunning = false;
						Console.WriteLine(cpu.GetCPUStateInfo());
						break;
					}
					//CheckSCRegister();
				}
				cpu.cycles = 0;
				Thread.Sleep(1);
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

    }
}

