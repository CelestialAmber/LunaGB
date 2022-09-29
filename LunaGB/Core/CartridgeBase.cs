using System;
namespace LunaGB.Core {

    //Base class for all cartridge types. Used to handle the different MBC chips used in different ROMs.
    public abstract class CartridgeBase {

        public int banks;
        public byte[] rom;
        public int currentBank; //Current loaded bank (bank 1 on gb)

        public CartridgeBase() {
        }

        public abstract byte GetByte(int index);
    }
}

