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

		public bool isRunning = false;
		public bool paused = false;


		public Emulator()
		{
			ppu = new PPU();
			display = new Display();
            rom = new ROM();
            memory = new Memory(rom);
            cpu = new CPU(memory);
        }

		//Loads the specified ROM.
		public void LoadROM(string romPath) {
			rom.OpenROM(romPath);
        }

		//Starts the emulator.
		public void Start() {
			isRunning = true;
			cpu.Init();
			memory.Init();
			Run();
        }

        public void Run() {
            while (isRunning) {
            	cpu.ExecuteInstruction();
				Thread.Sleep(1000);
            }
        }

		//Stops the emulator.
		public void Stop() {
            isRunning = false;
        }

    }
}

