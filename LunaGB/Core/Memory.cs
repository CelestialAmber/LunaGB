using System;
using LunaGB.Core.Debug;

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
		public Debugger debugger;

		public delegate void MemoryReadWriteEvent(int address);
		public event MemoryReadWriteEvent OnMemoryRead;
		public event MemoryReadWriteEvent OnMemoryWrite;

		public bool writeErrorOccured = false;


		public Memory(ROM rom, Debugger debugger)
		{
			this.rom = rom;
			this.debugger = debugger;
			OnMemoryRead += debugger.OnMemoryRead;
			OnMemoryWrite += debugger.OnMemoryWrite;
		}

		public void Init(){
			//Reset the different memory section arrays
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

			writeErrorOccured = false;
		}

		//Gets the byte located at the given address.
		public byte GetByte(int address) {
			//If breakpoints are enabled, invoke the event to check if any of the breakpoints were hit.
			if (debugger.breakpointsEnabled && !debugger.stepping)
			{
				OnMemoryRead?.Invoke(address);
			}

			if(address < 0x4000){
				//ROM Bank Slot 0
				//0000-3FFF
				if(rom.loadedRom){
				return rom.romMapper.GetByte(address);
				} else return 0;
			}else if(address < 0x8000){
				//ROM Bank Slot 1
				//4000-7FFF
				if(rom.loadedRom){
				return rom.romMapper.GetByte(address);
				}else return 0;
			}else if(address < 0xA000){
				//VRAM
				//8000-9FFF
				return vram[address - 0x8000];
			}else if(address < 0xC000){
				//Cartridge RAM Bank
				//A000-BFFF
				//Console.WriteLine("Tried to read from cartridge ram which isn't implemented yet");
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
				//HRAM and I/O Registers
				//FF00-FFFF
				if(address == 0xFFFF) {
					//Interrupt enable flag (IE)
					//0xFFFF
					return hram[address - 0xFF00];
				}else if(address >= 0xFF80) {
					//HRAM (0xFF80-FFFE)
					return hram[address - 0xFF00];
				} else if(IsIOReg(address - 0xFF00)) {
					IORegister reg = (IORegister)(address - 0xFF00);
					return GetIOReg(reg);
				} else {
					//If the address isn't an i/o register or in hram, reading it just returns 0?
					//TODO: determine what this should return
					return 0;
				}
			}

			return 0;
		}

		public void WriteByte(int address, byte b) {
			try{
			//If breakpoints are enabled, invoke the event to check if any of the breakpoints were hit.
			if (debugger.breakpointsEnabled && !debugger.stepping)
			{
				OnMemoryWrite?.Invoke(address);
			}

			if (address < 0x4000){
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
				//throw new NotImplementedException("Tried to write to cartridge ram, which isn't implemented yet");
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
				if(address == 0xFFFF) {
					//Interrupt enable flag (IE)
					//0xFFFF
					hram[address - 0xFF00] = b;
				} else if(address >= 0xFF80) {
					//HRAM (0xFF80-FFFE)
					hram[address - 0xFF00] = b;
				} else if(IsIOReg(address - 0xFF00)) {
					IORegister reg = (IORegister)(address - 0xFF00);
					SetIOReg(reg, b);
				} else {
					//If the address isn't an i/o register or in hram, writing does nothing?
				}
			}
			}catch(Exception e){
				Console.WriteLine(e.Message);
				writeErrorOccured = true;
			}
		}

		//Checks whether the given address is an i/o register or not
		bool IsIOReg(int regIndex) {
			return Enum.IsDefined(typeof(IORegister), regIndex);
		}

		//TODO: check whether the registers can be read/written to
		public byte GetIOReg(IORegister reg){
			int index = (int)reg;
			return hram[index];
		}

		public void SetIOReg(IORegister reg, byte val) {
			int index = (int)reg;
			hram[index] = val;
		}

		public byte GetHRAMBit(int bit, int address) {
			return (byte)((hram[address - 0xFF00] >> bit) & 1);
		}

		public void	SetHRAMBit(int address, int bit, int val) {
			byte b = hram[address - 0xFF00];
			hram[address - 0xFF00] = (byte)((b & ~(1 << bit)) | (val << bit));
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

