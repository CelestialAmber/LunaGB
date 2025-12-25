using System;
namespace LunaGB {

	//Base class for all cartridge types. Used to handle the different MBC chips used in different ROMs.
	public abstract class Cartridge {
		public byte[] rom;
		public byte[] ram;
		public int romBanks; //Number of rom banks
		public int ramBanks; //Number of ram banks
		public bool hasBattery; //if the cartridge has a battery
		public bool hasRam; //if the cartridge has builtin ram
		public bool hasTimer; //if the cartridge has a builtin timer
		public bool hasRumble; //if the cartridge has a rumble motor
		public bool sramDirty; //set whenever sram has been modified to signal the emulator to update the save file


		public Cartridge() {
			rom = Array.Empty<byte>();
			ram = Array.Empty<byte>();
		}

		public virtual void Init(){
			ClearRAM();
		}

		public virtual int GetSaveFileSize(){
			return ram.Length;
		}

		//Called when the emulator is paused/resumed to pause/resume the timer if necessary.
		public virtual void ToggleTimer(bool paused){
		}

		public abstract byte GetByte(int index);
		public abstract void SetByte(int index, byte val);

		public void ClearRAM(){
			if(hasRam){
				for(int i = 0; i < ram.Length; i++){
					ram[i] = 0;
				}
			}
		}
	}
}

