using System;
using LunaGB.Core.Debug;

namespace LunaGB.Core
{

	//Represents an object in OAM.
	public struct ObjectAttributes{
		public byte x;
		public byte y;
		public byte tileIndex;
		public byte flags;
		public int id;

		public ObjectAttributes(){
			x = 0;
			y = 0;
			tileIndex = 0;
			flags = 0;
			id = 0;
		}

		/*
		Flags:
		bits 0-2: cgb palette (gbc only)
		bit 3: vram bank (gbc only)
		bit 4: dmg palette (gb only)
		bit 5: x flip
		bit 6: y flip
		bit 7: priority
		*/
		public int palette => (flags >> 4) & 1;
		public bool xFlip => ((flags >> 5) & 1) == 1 ? true : false;
		public bool yFlip => ((flags >> 6) & 1) == 1 ? true : false;
		public int priority => (flags >> 7) & 1;
	}

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
		public ObjectAttributes[] oam = new ObjectAttributes[40]; //0xFE00-FE9F

		public ROM rom;
		public Debugger debugger;

		//Events
		public delegate void MemoryReadWriteEvent(int address);
		public event MemoryReadWriteEvent OnMemoryRead;
		public event MemoryReadWriteEvent OnMemoryWrite;
		public delegate void LCDEnableEvent(bool state);
		public event LCDEnableEvent? OnLCDEnableChange;
		public delegate void MemoryErrorEvent();
		public event MemoryErrorEvent? OnMemoryError;


		public bool canAccessOAM = false;
		public bool canAccessVRAM = false;
		public bool doingDMATransfer = false;


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
			for(int i = 0; i < 40; i++){
				oam[i] = new ObjectAttributes();
			}

			//Init the JOYP register
			hram[(int)IORegister.P1] = 0xFF;
			//Init the LCDC register
			SetHRAMBit((int)IORegister.LCDC, 7, 1);
			//Init the SB register (all 1s for now)
			hram[(int)IORegister.SB] = 0xFF;
			//Init the DIV register
			hram[(int)IORegister.DIV] = 0xAB;

			canAccessOAM = true;
			canAccessVRAM = true;
			doingDMATransfer = false;
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
				if(canAccessVRAM){
					return vram[address - 0x8000];
				}else{
					return 0xFF; //if vram can't be accessed right now (ppu is in drawing mode), return 0xFF?
				}
			}else if(address < 0xC000){
				//Cartridge RAM Bank
				//A000-BFFF
				if(rom.loadedRom){
					return rom.romMapper.GetByte(address);
				}
				//Console.WriteLine("Tried to read from cartridge ram which isn't implemented yet");
			}else if(address < 0xD000){
				//wram bank slot 0 (wram bank 0)
				//C000-CFFF
				return wram[address - 0xC000];
			}else if(address < 0xE000){
				//wram bank slot 1 (switchable)
				//D000-DFFF
				//TODO: actually handle wram banking
				return wram[address - 0xC000];
			}else if(address < 0xF000){
				//mirror of wram bank slot 0 (echo ram)
				//E000-EFFF
				return wram[address - 0xE000];
			}else if(address < 0xFE00){
				//mirror of wram bank slot 1 (echo ram)
				//F000-FDFF
				return wram[address - 0xE000];
			}else if(address < 0xFEA0){
				//OAM
				//FE00-FE9F
				if(canAccessOAM && !doingDMATransfer){
					return ReadFromOAM(address - 0xFE00);
				}else{
					return 0xFF; //If OAM can't be accessed right now (during OAM DMA/PPU modes 2/3), return 0xFF?
				}
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
				if(rom.loadedRom){
					rom.romMapper.SetByte(address,b);
				}
			}else if(address < 0x8000){
				//ROM Bank Slot 1
				//4000-7FFF
				if(rom.loadedRom){
					rom.romMapper.SetByte(address,b);
				}
			}else if(address < 0xA000){
				//VRAM
				//8000-9FFF
				//Only allow vram to be written to if it's accessible right now (ppu not in drawing mode)
				if(canAccessVRAM){
					vram[address - 0x8000] = b;
				}
			}else if(address < 0xC000){
				//Cartridge RAM Bank
				//A000-BFFF
				rom.romMapper.SetByte(address, b);
			}else if(address < 0xD000){
				//wram bank slot 0 (wram bank 0)
				//C000-CFFF
				wram[address - 0xC000] = b;
			}else if(address < 0xE000){
				//wram bank slot 1 (switchable)
				//D000-DFFF

				//TODO: actually handle wram banking
				wram[address - 0xC000] = b;
			}else if(address < 0xF000){
				//mirror of wram bank slot 0 (echo ram)
				//E000-EFFF
				wram[address - 0xE000] = b;
			}else if(address < 0xFE00){
				//mirror of wram bank slot 1 (echo ram)
				//F000-FDFF
				wram[address - 0xE000] = b;
			}else if(address < 0xFEA0){
				//OAM
				//FE00-FE9F
				//Only allow OAM to be written to if it's accessible (vblank/hblank, not in middle of oam dma)
				if(canAccessOAM && !doingDMATransfer){
					WriteToOAM(address - 0xFE00, b);
				}
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
				OnMemoryError?.Invoke();
			}
		}

		//Checks whether the given address is an i/o register or not
		bool IsIOReg(int regIndex) {
			return Enum.IsDefined(typeof(IORegister), regIndex);
		}

		//TODO: check whether the registers can be read/written to
		public byte GetIOReg(IORegister reg){
			if(reg == IORegister.P1){
				Input.UpdateJOYP();
			}
			int index = (int)reg;
			return hram[index];
		}

		public void SetIOReg(IORegister reg, byte val) {
			int index = (int)reg;

			switch(reg){
				case IORegister.P1:
				//Only bits 4/5 are read/writeable
				SetHRAMBit((int)IORegister.P1,4,(val >> 4) & 1);
				SetHRAMBit((int)IORegister.P1,5,(val >> 5) & 1);
				break;
				case IORegister.DIV:
				//If the CPU tries to write to the DIV register, reset it
				ResetDIV();
				break;
				case IORegister.DMA:
				//If the DMA register is written to, notify the emulator to perform an OAM DMA transfer
				doingDMATransfer = true;
				dmaTransferIndex = 0;
				//TODO: handle case where upper byte > 0xDF
				dmaTransferSourceAddress = val << 8;
				break;
				case IORegister.LCDC:
				int newLcdEnableValue = (val >> 7) & 1;
				int curLcdEnableValue = GetHRAMBit(7,(int)IORegister.LCDC);
				//If the lcd enable flag was changed, notify the emulator
				if(newLcdEnableValue != curLcdEnableValue){
					OnLCDEnableChange?.Invoke(newLcdEnableValue == 1 ? true : false);
				}
				hram[index] = val;
				break;
				default:
				hram[index] = val;
				break;
			}
		}

		int dmaTransferIndex = 0;
		int dmaTransferSourceAddress;

		//Performs a step for OAM DMA.
		public void OAMDMAStep(){
			WriteToOAM(dmaTransferIndex, GetByte(dmaTransferSourceAddress + dmaTransferIndex));
			dmaTransferIndex++;
			if(dmaTransferIndex == 160) doingDMATransfer = false;
		}

		//Helper functions to read/write values to/from the object array used for oam.
		byte ReadFromOAM(int offset){
			int byteIndex = offset % 4;
			int objectIndex = offset/4;

			if(byteIndex == 0) return oam[objectIndex].y;
			else if(byteIndex == 1) return oam[objectIndex].x;
			else if(byteIndex == 2) return oam[objectIndex].tileIndex;
			else return oam[objectIndex].flags;
		}

		void WriteToOAM(int offset, byte val){
			int byteIndex = offset % 4;
			int objectIndex = offset/4;
			
			if(byteIndex == 0) oam[objectIndex].y = val;
			else if(byteIndex == 1) oam[objectIndex].x = val;
			else if(byteIndex == 2) oam[objectIndex].tileIndex = val;
			else oam[objectIndex].flags = val;
		}

		public void RequestInterrupt(Interrupt interrupt){
			SetHRAMBit((int)IORegister.IF, (int)interrupt, 1);
		}

		public void ResetDIV(){
			hram[(int)IORegister.DIV] = 0;
		}

		public byte GetHRAMBit(int bit, int index) {
			return (byte)((hram[index] >> bit) & 1);
		}

		public void	SetHRAMBit(int index, int bit, int val) {
			byte b = hram[index];
			hram[index] = (byte)((b & ~(1 << bit)) | (val << bit));
		}

		public void WriteUInt16(int address, ushort val) {
			byte lowByte = (byte)(val & 0xFF), highByte = (byte)(val >> 8);
			WriteByte(address,lowByte);
			WriteByte((address + 1) % 0x10000,highByte);
		}

		public ushort GetUInt16(int address) {
			byte lowByte = GetByte(address);
			byte highByte = GetByte(address + 1);
			return (ushort)(lowByte + (highByte << 8));
		}
	}
}

