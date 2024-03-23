using System;


namespace LunaGB.Core.ROMMappers
{

//Class for MBC3 roms.
public class MBC3 : Cartridge
{
	int currentRomBank;
	int ramRtcSelectionReg;
	byte rtcSecs;
	byte rtcMins;
	byte rtcHours;
	bool haltRtc = false;
	//lower 8 bits of day counter
	byte rtcDayCounterLow;
	//bit 0: 9th bit of day counter
	//bit 6: halt (0: active, 1: stop timer)
	//bit 7: day counter carry bit (1: counter overflow)
	byte rtcDayCounterHigh;
	bool ramTimerEnable = false;

	//Used for handling latching the clock data
	int latchClockData = -1;
	bool clockLatched = false;

	public MBC3(bool hasRam, bool hasBattery, bool hasTimer){
		this.hasRam = hasRam;
		this.hasBattery = hasBattery;
		this.hasTimer = hasTimer;
	}

	public override void Init(){
		currentRomBank = 1;
		ramRtcSelectionReg = 0;
		rtcSecs = 0;
		rtcMins = 0;
		rtcHours = 0;
		rtcDayCounterLow = 0;
		rtcDayCounterHigh = 0;
		latchClockData = -1;
		clockLatched = false;
		haltRtc = false;
		ramTimerEnable = false;
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
			//External RAM/RTC Registers (0xA000-0xBFFF)
			//If the ram/rtc selection value is 0-3, access the corresponding ram bank.
			if(ramRtcSelectionReg <= 3){
				int bank = ramRtcSelectionReg;
				if(bank >= ramBanks) bank %= ramBanks;
				return ram[bank*0x2000 + (address - 0xA000)];
			}else{
				//If it's 8-12, access the corresponding RTC register instead.
				switch(ramRtcSelectionReg){
					case 8: return rtcSecs;
					case 9: return rtcMins;
					case 10: return rtcHours;
					case 11: return rtcDayCounterLow;
					case 12: return rtcDayCounterHigh;
					default: throw new Exception("how tf did it get here?");
				}
			}
		}else{
			//All other reads return 0?
			return 0xFF;
		}
    }

    public override void SetByte(int index, byte val) {
		if(index < 0x2000){
			//RAM/RTC Enable (0x0000-0x1FFF)
			if((val & 0xF) == 0xA){
				//RAM/RTC are enabled if the lower nybble is A
				if(hasRam || hasTimer) ramTimerEnable = true;
			}else{
				//Otherwise it's disabled
				ramTimerEnable = false;
			}
		}else if(index < 0x4000){
			//ROM Bank Number (0x2000-0x3FFF)
			//Only the lower 7 bits are read
			byte newBank = (byte)(val & 0b01111111);
			if(newBank == 0) newBank = 1;
			currentRomBank = newBank;
		}else if(index < 0x6000){
			//RAM Bank Number/RTC register select (0x4000-0x5FFF)
			if((val <= 3 && hasRam) || (val >= 8 && val <= 12 && hasTimer)){
				ramRtcSelectionReg = val;
			}
		}else if(index < 0x8000){
			//Latch Clock Data (0x6000-0x7FFF)
			//If 0x00 then 0x01 is written, the current time is kept in the rtc registers,
			//until the same process is repeated. Afterwards, the time in the rtc registers
			//is restored to the current time?

			//If 0 was written and 1 is now written, enable/disable latching the current time
			//to the rtc registers.
			if(latchClockData == 0 && val == 1){
				clockLatched = !clockLatched;
				if(clockLatched){
					UpdateRTC();
				}
			}
			latchClockData = val;
		}else if(index >= 0xA000 && index < 0xC000){
			//External RAM/RTC Registers (0xA000-0xBFFF)
			if(ramTimerEnable){
				//If the ram/rtc selection value is 0-3, access the corresponding ram bank.
				if(ramRtcSelectionReg <= 3){
					int bank = ramRtcSelectionReg;
					if(bank >= ramBanks) bank %= ramBanks;
					ram[bank*0x2000 + (index - 0xA000)] = val;
					//If the rom uses sram, mark it as dirty to signal the emulator to update the save file
					if(hasBattery) sramDirty = true;
				}else{
					//If it's 8-12, access the corresponding RTC register instead.
					//TODO
				}
			}
		}else{
			//Writes to other areas do nothing?
		}
	}

	void UpdateRTC(){
		//Set to a set value for now
		rtcSecs = 10;
		rtcMins = 23;
		rtcHours = 8;
		rtcDayCounterLow = 1;
		rtcDayCounterHigh = 0;
	}
}
}
