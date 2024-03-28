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
		public byte[] hram = new byte[0x7F]; //0xFF80-FFFE
		public byte[] wram = new byte[0x2000]; //0xC000-DFFF
		public ObjectAttributes[] oam = new ObjectAttributes[40]; //0xFE00-FE9F
		public Registers regs; //0xFF00-FF7F (I/O regs), 0xFFFF (IE flag)

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
		public delegate void SerialTransferEnableEvent();
		public event SerialTransferEnableEvent? OnSerialTransferEnable;

		public bool canAccessOAM = false;
		public bool canAccessVRAM = false;
		public bool doingDMATransfer = false;


		public Memory(ROM rom, Debugger debugger)
		{
			this.rom = rom;
			this.debugger = debugger;
			OnMemoryRead += debugger.OnMemoryRead;
			OnMemoryWrite += debugger.OnMemoryWrite;
			regs = new Registers();
		}

		public void Init(){
			//Reset the different memory section arrays
			//TODO: wram/hram should be randomized
			for(int i = 0; i < 0x2000; i++){
				vram[i] = 0;
			}
			for(int i = 0; i < 0x7F; i++){
				hram[i] = 0;
			}
			for(int i = 0; i < 0x2000; i++){
				wram[i] = 0;
			}
			for(int i = 0; i < 40; i++){
				oam[i] = new ObjectAttributes();
			}

			regs.Init();

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
					return regs.IE;
				}else if(address >= 0xFF80) {
					//HRAM (0xFF80-FFFE)
					return hram[address - 0xFF80];
				}else if(address >= 0xFF30 && address < 0xFF40){
					//Wave RAM (0xFF30-FF40)
					return regs.waveRam[address - 0xFF30];
				}else if(IsIOReg(address - 0xFF00)) {
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
				if(address == 0xFFFF) {
					//Interrupt enable flag (IE)
					//0xFFFF
					regs.IE = b;
				} else if(address >= 0xFF80) {
					//HRAM (0xFF80-FFFE)
					hram[address - 0xFF80] = b;
				}else if(address >= 0xFF30 && address < 0xFF40){
					//Wave RAM (0xFF30-FF40)
					regs.waveRam[address - 0xFF30] = b;
				}else if(IsIOReg(address - 0xFF00)) {
					//I/O registers (0xFF00-FF7F)
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

		public byte GetIOReg(IORegister reg){
			byte result = 0xFF;

			switch(reg){
				case IORegister.P1:
				Input.UpdateJOYP();
				result = regs.P1;
				break;
				case IORegister.SB:
				result = regs.SB;
				break;
				case IORegister.SC:
				result = regs.SC;
				break;
				case IORegister.DIV:
				result = regs.DIV;
				break;
				case IORegister.TIMA:
				result = regs.TIMA;
				break;
				case IORegister.TMA:
				result = regs.TMA;
				break;
				case IORegister.TAC:
				result = regs.TAC;
				break;
				case IORegister.IF:
				result = regs.IF;
				break;
				case IORegister.NR10:
				result = regs.NR10;
				break;
				case IORegister.NR11:
				result = regs.NR11;
				break;
				case IORegister.NR12:
				result = regs.NR12;
				break;
				case IORegister.NR13:
				result = regs.NR13;
				break;
				case IORegister.NR14:
				result = regs.NR14;
				break;
				case IORegister.NR21:
				result = regs.NR21;
				break;
				case IORegister.NR22:
				result = regs.NR22;
				break;
				case IORegister.NR23:
				result = regs.NR23;
				break;
				case IORegister.NR24:
				result = regs.NR24;
				break;
				case IORegister.NR30:
				result = regs.NR30;
				break;
				case IORegister.NR31:
				result = regs.NR31;
				break;
				case IORegister.NR32:
				result = regs.NR32;
				break;
				case IORegister.NR33:
				result = regs.NR33;
				break;
				case IORegister.NR34:
				result = regs.NR34;
				break;
				case IORegister.NR41:
				result = regs.NR41;
				break;
				case IORegister.NR42:
				result = regs.NR42;
				break;
				case IORegister.NR43:
				result = regs.NR43;
				break;
				case IORegister.NR44:
				result = regs.NR44;
				break;
				case IORegister.NR50:
				result = regs.NR50;
				break;
				case IORegister.NR51:
				result = regs.NR51;
				break;
				case IORegister.NR52:
				result = regs.NR52;
				break;
				case IORegister.LCDC:
				result = regs.LCDC;
				break;
				case IORegister.STAT:
				result = regs.STAT;
				break;
				case IORegister.SCY:
				result = regs.SCY;
				break;
				case IORegister.SCX:
				result = regs.SCX;
				break;
				case IORegister.LY:
				result = regs.LY;
				break;
				case IORegister.LYC:
				result = regs.LYC;
				break;
				case IORegister.DMA:
				result = regs.DMA;
				break;
				case IORegister.BGP:
				result = regs.BGP;
				break;
				case IORegister.OBP0:
				result = regs.OBP0;
				break;
				case IORegister.OBP1:
				result = regs.OBP1;
				break;
				case IORegister.WY:
				result = regs.WY;
				break;
				case IORegister.WX:
				result = regs.WX;
				break;
				case IORegister.KEY1:
				//result = regs.KEY1;
				break;
				case IORegister.VBK:
				//result = regs.VBK;
				break;
				case IORegister.HDMA1:
				//result = regs.HDMA1;
				break;
				case IORegister.HDMA2:
				//result = regs.HDMA2;
				break;
				case IORegister.HDMA3:
				//result = regs.HDMA3;
				break;
				case IORegister.HDMA4:
				//result = regs.HDMA4;
				break;
				case IORegister.HDMA5:
				//result = regs.HDMA5;
				break;
				case IORegister.RP:
				//result = regs.RP;
				break;
				case IORegister.BCPS:
				//result = regs.BCPS;
				break;
				case IORegister.BCPD:
				//result = regs.BCPD;
				break;
				case IORegister.OCPS:
				//result = regs.OCPS;
				break;
				case IORegister.OCPD:
				//result = regs.OCPD;
				break;
				case IORegister.SVBK:
				//result = regs.SVBK;
				break;
				case IORegister.PCM12:
				//result = regs.PCM12;
				break;
				case IORegister.PCM34:
				//result = regs.PCM34;
				break;
				default:
				Console.WriteLine("Error: somehow trying to read from a missing io register ;<");
				break;
			}

			return result;
		}

		public void SetIOReg(IORegister reg, byte val) {
			switch(reg){
				case IORegister.P1:
				regs.P1 = val;
				break;
				case IORegister.SB:
				regs.SB = val;
				break;
				case IORegister.SC:
				int newTransferEnableVal = (val >> 7) & 1;
				int curTransferEnableVal = regs.GetBit(regs.SC, 7);
				//If transfer enable is newly set, enable serial transfer
				if(curTransferEnableVal == 0 && newTransferEnableVal == 1){
					OnSerialTransferEnable?.Invoke();
				}
				regs.SC = (byte)(val & 0b10000011);
				break;
				case IORegister.DIV:
				//If the CPU tries to write to the DIV register, reset it
				ResetDIV();
				break;
				case IORegister.TIMA:
				regs.TIMA = val;
				break;
				case IORegister.TMA:
				regs.TMA = val;
				break;
				case IORegister.TAC:
				regs.TAC = val;
				break;
				case IORegister.IF:
				regs.IF = val;
				break;
				case IORegister.NR10:
				regs.NR10 = val;
				break;
				case IORegister.NR11:
				regs.NR11 = val;
				break;
				case IORegister.NR12:
				regs.NR12 = val;
				break;
				case IORegister.NR13:
				regs.NR13 = val;
				break;
				case IORegister.NR14:
				regs.NR14 = val;
				break;
				case IORegister.NR21:
				regs.NR21 = val;
				break;
				case IORegister.NR22:
				regs.NR22 = val;
				break;
				case IORegister.NR23:
				regs.NR23 = val;
				break;
				case IORegister.NR24:
				regs.NR24 = val;
				break;
				case IORegister.NR30:
				regs.NR30 = val;
				break;
				case IORegister.NR31:
				regs.NR31 = val;
				break;
				case IORegister.NR32:
				regs.NR32 = val;
				break;
				case IORegister.NR33:
				regs.NR33 = val;
				break;
				case IORegister.NR34:
				regs.NR34 = val;
				break;
				case IORegister.NR41:
				regs.NR41 = val;
				break;
				case IORegister.NR42:
				regs.NR42 = val;
				break;
				case IORegister.NR43:
				regs.NR43 = val;
				break;
				case IORegister.NR44:
				regs.NR44 = val;
				break;
				case IORegister.NR50:
				regs.NR50 = val;
				break;
				case IORegister.NR51:
				regs.NR51 = val;
				break;
				case IORegister.NR52:
				regs.NR52 = val;
				break;
				case IORegister.LCDC:
				int newLcdEnableValue = (val >> 7) & 1;
				int curLcdEnableValue = regs.lcdcEnable;
				//If the lcd enable flag was changed, notify the emulator
				if(newLcdEnableValue != curLcdEnableValue){
					OnLCDEnableChange?.Invoke(newLcdEnableValue == 1 ? true : false);
				}
				regs.LCDC = val;
				break;
				case IORegister.STAT:
				regs.STAT = val;
				break;
				case IORegister.SCY:
				regs.SCY = val;
				break;
				case IORegister.SCX:
				regs.SCX = val;
				break;
				case IORegister.LY:
				//LY is read only
				Console.WriteLine("When the ROM is sus! 😳 (LY is read only silly)");
				break;
				case IORegister.LYC:
				regs.LYC = val;
				break;
				case IORegister.DMA:
				//If the DMA register is written to, notify the emulator to perform an OAM DMA transfer
				doingDMATransfer = true;
				dmaTransferIndex = 0;
				//TODO: handle case where upper byte > 0xDF
				dmaTransferSourceAddress = val << 8;
				//regs.DMA = val;
				break;
				case IORegister.BGP:
				regs.BGP = val;
				break;
				case IORegister.OBP0:
				regs.OBP0 = val;
				break;
				case IORegister.OBP1:
				regs.OBP1 = val;
				break;
				case IORegister.WY:
				regs.WY = val;
				break;
				case IORegister.WX:
				regs.WX = val;
				break;
				case IORegister.KEY1:
				//regs.KEY1 = val;
				break;
				case IORegister.VBK:
				//regs.VBK = val;
				break;
				case IORegister.HDMA1:
				//regs.HDMA1 = val;
				break;
				case IORegister.HDMA2:
				//regs.HDMA2 = val;
				break;
				case IORegister.HDMA3:
				//regs.HDMA3 = val;
				break;
				case IORegister.HDMA4:
				//regs.HDMA4 = val;
				break;
				case IORegister.HDMA5:
				//regs.HDMA5 = val;
				break;
				case IORegister.RP:
				//regs.RP = val;
				break;
				case IORegister.BCPS:
				//regs.BCPS = val;
				break;
				case IORegister.BCPD:
				//regs.BCPD = val;
				break;
				case IORegister.OCPS:
				//regs.OCPS = val;
				break;
				case IORegister.OCPD:
				//regs.OCPD = val;
				break;
				case IORegister.SVBK:
				//regs.SVBK = val;
				break;
				case IORegister.PCM12:
				//regs.PCM12 = val;
				break;
				case IORegister.PCM34:
				//regs.PCM34 = val;
				break;
				default:
				Console.WriteLine("Error: somehow trying to write to a missing io register :<");
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
			regs.SetBit(ref regs.IF, (int)interrupt, 1);
		}

		public void ResetDIV(){
			regs.DIV = 0;
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

