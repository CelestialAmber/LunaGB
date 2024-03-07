using System;

namespace LunaGB.Core
{

	public class Input
	{
		public enum Button {
			Right,
			Left,
			Up,
			Down,
			A,
			B,
			Select,
			Start
		}

		public static int[] flags = {1,1,1,1,1,1,1,1};

		public static Memory memory;

		public static void UpdateJOYP(){
			//If the select dpad bit is 0, set dpad buttons 
			if(memory.GetHRAMBit(4,(int)IORegister.P1) == 0){
				memory.hram[(int)IORegister.P1] |= (byte)(flags[0] | (flags[1] << 1) | (flags[2] << 2) | (flags[3] << 3));
			}else if(memory.GetHRAMBit(5,(int)IORegister.P1) == 0){
				//If the select buttons bit is 0, set select buttons
				memory.hram[(int)IORegister.P1] |= (byte)(flags[4] | (flags[5] << 1) | (flags[6] << 2) | (flags[7] << 3));
			}else{
				//If neither bits 4/5 are enabled, set the lower nybble to all ones
				memory.hram[(int)IORegister.P1] |= 0xF;
			}

			//Console.WriteLine(Convert.ToString(memory.hram[(int)IORegister.P1],2));
		}

		public static void OnButtonDown(Button button) {
			flags[(int)button] = 0;
			Console.WriteLine("Pressed " + button);
		}

		public static void OnButtonUp(Button button) {
			flags[(int)button] = 1;
			Console.WriteLine("Released " + button);
		}


	}
}

