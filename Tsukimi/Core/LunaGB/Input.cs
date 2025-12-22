using System;

namespace Tsukimi.Core.LunaGB
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

		public static Memory? memory;

		public static void UpdateJOYP(){
			//If the select dpad bit is 0, set dpad buttons 
			if(memory.regs.P14 == 0){
				memory.regs.P10 = flags[0];
				memory.regs.P11 = flags[1];
				memory.regs.P12 = flags[2];
				memory.regs.P13 = flags[3];
			}else if(memory.regs.P15 == 0){
				//If the select buttons bit is 0, set select buttons
				memory.regs.P10 = flags[4];
				memory.regs.P11 = flags[5];
				memory.regs.P12 = flags[6];
				memory.regs.P13 = flags[7];
			}else{
				//If neither bits 4/5 are enabled, set the lower nybble to all ones
				memory.regs.P10 = 1;
				memory.regs.P11 = 1;
				memory.regs.P12 = 1;
				memory.regs.P13 = 1;
			}
		}

		public static void OnButtonDown(Button button) {
			flags[(int)button] = 0;
		}

		public static void OnButtonUp(Button button) {
			flags[(int)button] = 1;
		}


	}
}

