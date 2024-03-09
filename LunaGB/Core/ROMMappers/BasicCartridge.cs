using System;
namespace LunaGB.Core.ROMMappers
{
    //Class for basic roms which don't use bankswitching/don't have a MBC.
    //Examples: Tetris, Dr. Mario, Alleyway
    public class BasicCartridge : Cartridge
    {

		public override byte GetByte(int index) {
            //The basic rom mapper doesn't support bank switching, so we can directly use the index

			//If reading from rom, return the corresponding byte in rom
			if(index < 0x8000){
            	return rom[index];
			}else{
				//Otherwise, return 0?
				return 0;
			}
        }

        public override void SetByte(int index, byte val) {
           // throw new Exception("Trying to write to ROM");
        }
    }
}

