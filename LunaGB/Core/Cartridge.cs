using System;
namespace LunaGB.Core {

	//Base class for all cartridge types. Used to handle the different MBC chips used in different ROMs.
	public abstract class Cartridge {

		public int banks;
		public byte[]rom;
		public int currentBank; //Current loaded bank (bank 1 on gb)
		public bool hasBattery; //if the cartridge has a battery
		public bool hasRam; //if the cartridge has builtin ram


		public Cartridge() {
		}

		public abstract byte GetByte(int index);
		public abstract void SetByte(int index, byte val);
	}
}

