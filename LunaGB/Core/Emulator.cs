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

		//Loads the specified ROM.
		public void LoadROM(string romPath) {
			rom.OpenROM(romPath);
        }

		//Starts the emulator.
		public void Start() {
			//isRunning = true;
			//Run();
        }

        public void Run() {
            while (isRunning) {
               // if (Input.IsButtonPressed(Button.A)) {
				//	Console.WriteLine("Pressed A");
                //}
				Thread.Sleep(1000);
            }
        }

    }
}

