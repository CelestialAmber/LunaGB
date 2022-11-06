using System;
namespace LunaGB.Core.ROMMappers
{
    //Class for basic roms which don't use bankswitching/don't have a MBC.
    //Examples: Tetris, Dr. Mario, Alleyway
    public class BasicCartridge : Cartridge
    {
        public BasicCartridge()
        {
        }

        public override byte GetByte(int index) {
            //The basic rom mapper doesn't support bank switching, so we can directly use the index
            return rom[index];
        }

        public override void SetByte(int index, byte val) {
            Console.WriteLine("Trying to write to ROM");
        }
    }
}

