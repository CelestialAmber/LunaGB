using System;
namespace LunaGB.Core
{
	public class Memory
	{
		/*
		The Game Boy has a 16-bit address space.

		Memory map:
		0000-3FFF: rom bank slot 0 (bank 0)
		4000-7FFF: rom bank slot 1 (bank 1-)
		8000-9FFF: vram (gb: bank 0 only, gbc: can switch between bank 0/1)
		A000-BFFF: external ram (cartridge ram, switchable)
		C000-CFFF: wram bank slot 0 (bank 0)
		D000-DFFF: wram bank slot 1 (bank 1-)
		E000-FDFF: echo ram (mirror of C000-DDFF)
		FE00-FE9F: oam
		FEA0-FEFF: unused
		FF00-FFFF: hram
		*/

		public byte[] vram = new byte[0x2000]; //0x8000-9FFF
		//FF00-FF7F: i/o registers, FF80-FFFE: hram, FFFF: interrupt flag (ie)
		public byte[] hram = new byte[0x100]; //0xFF00-FFFF
		public byte[] wram = new byte[0x2000]; //0xC000-DFFF
		public byte[] oam = new byte[0xA0]; //0xFE00-FE9F

		public ROM rom;


		public Memory(ROM rom)
		{
			this.rom = rom;
		}

		public void Init(){
			for(int i = 0; i < 0x2000; i++){
				vram[i] = 0;
			}
			for(int i = 0; i < 0x100; i++){
				hram[i] = 0;
			}
			for(int i = 0; i < 0x2000; i++){
				wram[i] = 0;
			}
			for(int i = 0; i < 0xA0; i++){
				oam[i] = 0;
			}
		}

		//Gets the byte located at the given address.
		public byte GetByte(int address) {
			if(address < 0x4000){
				//ROM Bank Slot 0
				//0000-3FFF
				return rom.romMapper.GetByte(address);
			}else if(address < 0x8000){
				//ROM Bank Slot 1
				//4000-7FFF
				return rom.romMapper.GetByte(address);
			}else if(address < 0xA000){
				//VRAM
				//8000-9FFF
				return vram[address - 0x8000];
			}else if(address < 0xC000){
				//Cartridge RAM Bank
				//A000-BFFF
				throw new NotImplementedException("Tried to read from cartridge ram which isn't implemented yet");
			}else if(address < 0xD000){
				//wram bank slot 0 (wram bank 0)
				//C000-CFFF
				return wram[address - 0xC000];
			}else if(address < 0xE000){
				//wram bank slot 1 (switchable)
				//D000-DFFF

				//TODO: actually handle wram banking
				return wram[address - 0xD000];
			}else if(address < 0xF000){
				//mirror of wram bank slot 0 (echo ram)
				//E000-EFFF
				return wram[address - 0xE000];
			}else if(address < 0xFE00){
				//mirror of wram bank slot 1 (echo ram)
				//F000-FDFF
				return wram[address - 0xF000];
			}else if(address < 0xFEA0){
				//OAM
				//FE00-FE9F
				return oam[address - 0xFE00];
			}else if(address < 0xFF00){
				//unusable space
				//FEA0-FEFF
			}else{
				//HRAM
				//FF00-FFFF
				//TODO: check if the unusable memory at 0xFF4C-0xFF7F shouldn't be allowed to be accessed
				return hram[address - 0xFF00];
			}

			return 0;
        }

		public void WriteByte(int address, byte b) {
			if(address < 0x4000){
				//ROM Bank Slot 0
				//0000-3FFF
				rom.romMapper.SetByte(address,b);
			}else if(address < 0x8000){
				//ROM Bank Slot 1
				//4000-7FFF
				rom.romMapper.SetByte(address,b);
			}else if(address < 0xA000){
				//VRAM
				//8000-9FFF
				//TODO: don't allow vram to be written when it shouldn't be
				vram[address - 0x8000] = b;
			}else if(address < 0xC000){
				//Cartridge RAM Bank
				//A000-BFFF
				throw new NotImplementedException("Tried to write to cartridge ram, which isn't implemented yet");
			}else if(address < 0xD000){
				//wram bank slot 0 (wram bank 0)
				//C000-CFFF
				wram[address - 0xC000] = b;
			}else if(address < 0xE000){
				//wram bank slot 1 (switchable)
				//D000-DFFF

				//TODO: actually handle wram banking
				wram[address - 0xD000] = b;
			}else if(address < 0xF000){
				//mirror of wram bank slot 0 (echo ram)
				//E000-EFFF
				wram[address - 0xE000] = b;
			}else if(address < 0xFE00){
				//mirror of wram bank slot 1 (echo ram)
				//F000-FDFF
				wram[address - 0xF000] = b;
			}else if(address < 0xFEA0){
				//OAM
				//FE00-FE9F
				//Can't be written to during OAM DMA?
				oam[address - 0xFE00] = b;
			}else if(address < 0xFF00){
				//unusable space
				//FEA0-FEFF
			}else{
				//HRAM
				//FF00-FFFF
				//TODO: check if the unusable memory at 0xFF4C-0xFF7F shouldn't be allowed to be accessed
				hram[address - 0xFF00] = b;
			}
        }

       public void WriteUInt16(int address, ushort val) {
			byte lowByte = (byte)(val & 0xFF), highByte = (byte)(val >> 8);
			WriteByte(address,lowByte);
			WriteByte(address + 1,highByte);
        }

		public ushort GetUInt16(int address) {
			byte lowByte = GetByte(address);
			byte highByte = GetByte(address + 1);
			return (ushort)(lowByte + (highByte << 8));
        }
	}
}

