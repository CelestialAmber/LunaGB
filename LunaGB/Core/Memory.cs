using System;
namespace LunaGB.Core
{
	public class Memory
	{
		public byte[] memory = new byte[0x10000]; //0x10000 bytes of ram

		public ROM rom;


		public Memory(ROM rom)
		{
			this.rom = rom;
		}

		public byte GetByte(int index) {
			return 0;
        }

		public void WriteByte(int index, byte b) {

        }

		public ushort GetUInt16(int index) {
			return 0;
        }
	}
}

