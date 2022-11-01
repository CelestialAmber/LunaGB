using System;
namespace LunaGB.Core.ROMMappers
{
    //Class for basic roms which don't use bankswitching/don't have a MBC.
    public class BasicCartridge : CartridgeBase
    {
        public BasicCartridge()
        {
        }

        public override byte GetByte(int index) {
            return 0;
        }

        public override void SetByte(int index, byte val) {

        }
    }
}

