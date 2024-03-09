using System;
namespace LunaGB.Core.ROMMappers
{

public class MBC1 : Cartridge
{

	//0: 8KB RAM max, 2MB ROM max (default)
	//1: 32KB RAM max, 512KB ROM max
	int bankingMode = 0;
	//Upper 2 bits for current bank (also used for bank slot 0 in banking mode 1)
	int currentBankUpperBits = 0;
	bool ramEnable = false;

	public MBC1(bool hasRam, bool hasBattery){
		this.hasRam = hasRam;
		this.hasBattery = hasBattery;
	}

	public override void Init(){
		currentBank = 0;
		currentRamBank = 0;
		currentBankUpperBits = 0;
		bankingMode = 0;
		ramEnable = false;
	}

	public override byte GetByte(int address) {
		if(address < 0x4000){
			//Bank 0 (0x0000-0x3FFF)
			//In banking mode 1, bank 0 also uses the upper 2 bank bits
			int romBank = bankingMode == 0 ? 0 : currentBankUpperBits << 5;
        	return rom[romBank*0x4000 + address];
		}else if(address < 0x8000){
			//Bank 1 (0x4000-0x7FFF)
			//The actual current bank is the bank value plus 1
			int romBank = currentBank;
			//Bank 00->01 translation
			//This ignores the top 2 bits, which causes issues
			if(romBank == 0) romBank = 1;
			if(bankingMode == 0) romBank += currentBankUpperBits << 5;
			return rom[romBank*0x4000 + (address - 0x4000)];
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
			if(val == 0){
				//RAM is disabled if the value is 0
				ramEnable = false;
			}else if((val & 0xF) == 0xA){
				//RAM is enabled if the lower nybble is A
				ramEnable = true;
			}
		}else if(index < 0x4000){
			//ROM Bank Number (0x2000-0x3FFF)
			//Only the lower 5 bits are read
			int newBank = val & 0b00011111;
			if(newBank >= romBanks){
				newBank = 0;
			}
			currentBank = newBank;
		}else if(index < 0x6000){
			//RAM Bank Number/Upper bits of ROM Bank Number (0x4000-0x5FFF)
			if(bankingMode == 0){
				//If the banking mode is 0 (ROM banking mode), the value
				//specifies the top 2 bits of the ROM bank number.
				int newBankVal = (val << 5) + currentBank;
				//If the new bank is valid, update the current rom bank
				if(newBankVal < romBanks){
					currentBankUpperBits = val;
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
			//If neither rom or ram is big enough, changing the mode does nothing
			if(romBanks <= 32 && ramBanks == 1) return;
			bankingMode = val;
		}else if(index >= 0xA000 && index < 0xC000){
			//External RAM (0xA000-0xBFFF) (todo)
			if(ramEnable){
				int ramOffset = index - 0xA000;
				//If the cartridge only has 2kb and the game tries to write
				//past it, don't do anything
				if(has2KBRam && ramOffset >= 0x800) return;
				
				//Otherwise, write the corresponding byte in ram
				ram[currentRamBank*0x4000 + ramOffset] = val;
			}
		}else{
			//Writes to other areas do nothing?
		}
	}
}
}
