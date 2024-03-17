using System;
namespace LunaGB.Core.ROMMappers
{

//Class for MBC2 roms.
public class MBC2 : Cartridge
{
	int currentRomBank;
	bool ramEnable;

	public MBC2(bool hasBattery){
		hasRam = true; //MBC2 has builtin ram
		this.hasBattery = hasBattery;
		//512 4bit values (only lower 4 bits of each are used)
		ram = new byte[512];
	}

	public override void Init(){
		currentRomBank = 1;
		ramEnable = false;
	}

	public override byte GetByte(int address) {
		if(address < 0x4000){
			//Bank 0 (0x0000-0x3FFF)
        	return rom[address];
		}else if(address < 0x8000){
			//Bank 1 (0x4000-0x7FFF)
			int romBank = currentRomBank;
			if(romBank >= romBanks) romBank %= romBanks;
			return rom[romBank*0x4000 + (address - 0x4000)];
		}else if(address >= 0xA000 && address < 0xC000){
			//External RAM (0xA000-0xBFFF)
			/*
			After 0xA1FF, the same values repeat every 512 bytes.
			(e.g. 0xA200-0xA3FF, 0xA400-0xA5FF... = 0xA000-0xA1FF)
			*/
			if(ramEnable){
				int index = (address - 0xA000) % 512;
				//In reality the top 4 bits are undefined, but apparently
				//tests expect them to all be 1.
				return (byte)((ram[index] & 0xF) | 0xF0);
			}else{
				//If ram isn't enabled, return 0?
				return 0xFF;
			}
		}else{
			//All other reads return 0?
			return 0xFF;
		}
    }

    public override void SetByte(int address, byte val) {
		if(address < 0x4000){
			//RAM Enable/ROM Bank Number (0x0000-0x3FFF)
			//0: ram enable, 1: rom bank number
			int mode = (address >> 8) & 1;
			if(mode == 0){
				if((val & 0xF) == 0xA){
					//RAM is enabled if the lower nybble is A
					ramEnable = true;
				}else{
					//Otherwise it's disabled
					ramEnable = false;
				}
			}else{
				//Only the lower 4 bits are read
				int newBank = val & 0xF;
				if(newBank == 0) newBank = 1;
				currentRomBank = newBank;
			}
		}else if(address >= 0xA000 && address < 0xC000){
			//External RAM (0xA000-0xBFFF)
			/*
			After 0xA1FF, the rest of the address space just echoes the ram
			every 512 bytes. (e.g. 0xA200-0xA3FF, 0xA400-0xA5FF... = 0xA000-0xA1FF)
			*/
			if(ramEnable){
				int index = (address - 0xA000) % 512;
				ram[index] = (byte)(val & 0xF);
				//If the rom uses sram, mark it as dirty to signal the emulator to update the save file
				if(hasBattery) sramDirty = true;
			}
		}else{
			//Writes to other areas do nothing?
		}
	}
}
}
