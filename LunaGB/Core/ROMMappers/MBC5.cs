using System;
namespace LunaGB.Core.ROMMappers
{

//Class for MBC5 roms.
public class MBC5 : Cartridge
{
	int currentRomBank;
	int currentRamBank;
	bool ramEnable;
	bool rumbleEnabled;

	public MBC5(bool hasRam, bool hasBattery, bool hasRumble){
		this.hasRam = hasRam;
		this.hasBattery = hasBattery;
		this.hasRumble = hasRumble;
	}

	public override void Init(){
		currentRomBank = 1;
		currentRamBank = 0;
		rumbleEnabled = false;
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
			if(ramEnable){
				int bank = currentRamBank;
				if(bank >= ramBanks) bank %= ramBanks;
				return ram[bank*0x2000 + (address - 0xA000)];
			}else{
				return 0xFF;
			}
		}else{
			//All other reads return 0?
			return 0xFF;
		}
    }

    public override void SetByte(int index, byte val) {
		if(index < 0x2000){
			//RAM Enable (0x0000-0x1FFF)
			if((val & 0xF) == 0xA){
				//RAM is enabled if the lower nybble is A
				if(hasRam) ramEnable = true;
			}else{
				//Otherwise it's disabled
				ramEnable = false;
			}
		}else if(index < 0x3000){
			//Lower 8 bits of ROM Bank Number (0x2000-0x2FFF)
			currentRomBank = (currentRomBank & ~0xFF) | val;
		}else if(index < 0x4000){
			//9th bit of ROM Bank Number (0x3000-0x3FFF)
			currentRomBank = (currentRomBank & 0xFF) | ((val & 1) << 8);
		}else if(index < 0x6000){
			//RAM Bank Number/Rumble Enable (0x4000-0x5FFF)
			//The upper 4 bits are unused
			val &= 0xF;
			if(hasRam){
				currentRamBank = val & 0b111;
			}
			//If the cartridge has a rumble motor, bit 3 is used to enable/disable rumble.
			if(hasRumble){
				rumbleEnabled = ((val >> 3) & 1) == 1 ? true : false;
			}
		}else if(index >= 0xA000 && index < 0xC000){
			//External RAM (0xA000-0xBFFF)
			if(ramEnable){
				int bank = currentRamBank;
				if(bank >= ramBanks) bank %= ramBanks;
				ram[bank*0x2000 + (index - 0xA000)] = val;
			}
		}else{
			//Writes to other areas do nothing?
		}
	}
}
}
