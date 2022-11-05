using System;
namespace LunaGB.Core
{
	public class Memory
	{
		public byte[] vram = new byte[0x2000]; //0x8000-9FFF
		public byte[] hram = new byte[0xFF]; //0xFF00-FFFF
		public byte[] wram = new byte[0x2000]; //0xC000-DFFF
		public byte[] oam = new byte[0xA0]; //0xFE00-FE9F

		public ROM rom;


		public Memory(ROM rom)
		{
			this.rom = rom;
		}

		public byte GetByte(int index) {
			if(index < 0x4000){
				//get byte from bank 0
			}else if(index < 0x8000){
				//get byte from switchable bank
			}else if(index < 0xA000){
				//vram
			}else if(index < 0xC000){
				//switchable ram bank
				throw new NotImplementedException("Tried to read from switchable ram bank which isn't implemented yet");
			}else if(index < 0xE000){
				//wram
			}else if(index < 0xFE00){
				//unusable space
			}else if(index < 0xFEA0){
				//oam
			}else if(index < 0xFF00){
				//unusable space
			}else{
				//hram
				//TODO: check if the unusable memory at 0xFF4C-0xFF7F shouldn't be allowed to be accessed
			}

			return 0;
        }

		public void WriteByte(int index, byte b) {

        }

       public void WriteUInt16(int index, ushort val) {

        }

		public ushort GetUInt16(int index) {
			return 0;
        }
	}
}

