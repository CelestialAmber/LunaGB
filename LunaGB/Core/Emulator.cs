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

		bool isRunning = false;


		public Emulator()
		{
			ppu = new PPU();
			display = new Display();
            rom = new ROM();
            memory = new Memory(rom);
            cpu = new CPU(memory);
        }

		public void Run() {
            while (isRunning) {

            }
        }
	}
}

