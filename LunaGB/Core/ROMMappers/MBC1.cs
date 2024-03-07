using System;
namespace LunaGB.Core.ROMMappers
{

public class MBC1 : Cartridge
{

	//0: 8KB RAM max, 2MB ROM max (default)
	//1: 32KB RAM max, 512KB ROM max
	int bankingMode = 0;
	bool ramEnable = false;

	public MBC1(bool hasRam, bool hasBattery){
		this.hasRam = hasRam;
		this.hasBattery = hasBattery;
	}

	public override byte GetByte(int address) {
		if(address < 0x4000){
			//Bank 0 (0x0000-0x3FFF)
        	return rom[address];
		}else if(address < 0x8000){
			//Bank 1 (0x4000-0x7FFF)
			return rom[currentBank*0x4000 + (address - 0x4000)];
		}else if(address >= 0xA000 && address < 0xC000){
			//External RAM (0xA000-0xBFFF) (todo)
			if(ramEnable){
				int ramOffset = address - 0xA000;
				//If the cartridge only has 2kb and the game tries to read
				//past it, return 0?
				if(has2KBRam && ramOffset >= 0x800) return 0;
				else{
					//Otherwise, return the corresponding byte in ram
					return ram[currentRamBank*0x4000 + ramOffset];
				}
			}else{
				//If ram isn't enabled, return 0?
				return 0;
			}
		}else{
			//All other reads return 0?
			return 0;
		}
    }

    public override void SetByte(int index, byte val) {
		if(index < 0x2000){
			//RAM Enable (0x0000-0x1FFF)
			if(val == 0) ramEnable = false;
			else if(val == 10) ramEnable = true;
		}else if(index < 0x4000){
			//ROM Bank Number (0x2000-0x3FFF)
			//If the rom bank number is valid, update the current bank
			if(val < romBanks){
				currentBank = val;
			}
		}else if(index < 0x6000){
			//RAM Bank Number/Upper bits of ROM Bank Number (0x4000-0x5FFF)
			if(bankingMode == 0){
				//If the banking mode is 0 (ROM banking mode), the value
				//specifies the top 2 bits of the ROM bank number.
				int newBankVal = currentBank | ((val & 3) << 8);
				if(newBankVal < romBanks){
					currentBank = newBankVal;
				}
			}else{
				//If the banking mode is 1 (RAM banking mode), the value
				//instead specifies the RAM bank number.
				if(val < ramBanks){
					currentRamBank = val;
				}
			}
		}else if(index < 0x8000){
			//ROM/RAM Mode Select (0x6000-0x7FFF)
			bankingMode = val;
		}else{
			//Writes to other areas do nothing?
		}
	}
}
}
