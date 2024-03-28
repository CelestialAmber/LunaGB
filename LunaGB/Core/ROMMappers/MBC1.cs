using System;
namespace LunaGB.Core.ROMMappers
{

//Class for MBC1 roms, which accounts for most games released for the original Game Boy.
//Examples: Donkey Kong '94, Pokemon Red/Green (JP), Kirby's Dream Land
public class MBC1 : Cartridge
{

	//0: 8KB RAM max, 2MB ROM max (default)
	//1: 32KB RAM max, 512KB ROM max
	int bankingMode = 0;
	byte bank1 = 0;
	byte bank2 = 0;
	bool ramEnable = false;

	public MBC1(bool hasRam, bool hasBattery){
		this.hasRam = hasRam;
		this.hasBattery = hasBattery;
	}

	public override void Init(){
		bank1 = 1;
		bank2 = 0;
		bankingMode = 0;
		ramEnable = false;
	}

	public override byte GetByte(int address) {
		if(address < 0x4000){
			//Bank 0 (0x0000-0x3FFF)
			//Mode 0: bank 0, Mode 1: bank2 << 5
			int romBank = bankingMode == 0 ? 0 : bank2 << 5;
			if(romBank >= romBanks) romBank %= romBanks;
        	return rom[romBank*0x4000 + address];
		}else if(address < 0x8000){
			//Bank 1 (0x4000-0x7FFF)
			int romBank = bank1 + (bank2 << 5);
			if(romBank >= romBanks) romBank %= romBanks;
			return rom[romBank*0x4000 + (address - 0x4000)];
		}else if(address >= 0xA000 && address < 0xC000){
			//External RAM (0xA000-0xBFFF)
			if(ramEnable){
				//Mode 0: bank 0, Mode 1: value in bank2
				int bank = bankingMode == 0 ? 0 : bank2;
				if(bank >= ramBanks) bank %= ramBanks;
				return ram[bank*0x2000 + (address - 0xA000)];
			}else{
				//If ram isn't enabled, return 0?
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
				if(hasRam)ramEnable = true;
			}else{
				//Otherwise it's disabled
				ramEnable = false;
			}
		}else if(index < 0x4000){
			//ROM Bank Number (0x2000-0x3FFF)
			//Only the lower 5 bits are read
			byte newBank = (byte)(val & 0b00011111);
			//Bank 00->01 translation
			//This ignores the top 2 bits, which causes issues
			if(newBank == 0) newBank = 1;
			bank1 = newBank;
		}else if(index < 0x6000){
			//RAM Bank Number/Upper bits of ROM Bank Number (0x4000-0x5FFF)
			//Bits 2-7 are ignored
			bank2 = (byte)(val & 0b00000011);
		}else if(index < 0x8000){
			//ROM/RAM Mode Select (0x6000-0x7FFF)
			//If neither rom or ram is big enough, changing the mode does nothing
			if(romBanks <= 32 && ramBanks == 1) return;
			bankingMode = val;
		}else if(index >= 0xA000 && index < 0xC000){
			//External RAM (0xA000-0xBFFF)
			if(ramEnable){
				int bank = bankingMode == 0 ? 0 : bank2;
				if(bank >= ramBanks) bank %= ramBanks;
				ram[bank*0x2000 + (index - 0xA000)] = val;
				//If the rom uses sram, mark it as dirty to signal the emulator to update the save file
				if(hasBattery) sramDirty = true;
			}
		}else{
			//Writes to other areas do nothing?
		}
	}
}
}
