using System;

namespace LunaGB {

	public class Serial{
		Memory memory;
		int cycleCount = 0;
		bool doingTransfer = false;
		int shiftCount;


		public Serial(Memory memory){
			this.memory = memory;
		}

		public void Init(){
			cycleCount = 0;
			doingTransfer = false;
		}

		//Called when bit 7 of SC is set to 1.
		public void RequestTransfer(){
			doingTransfer = true;
			shiftCount = 8;
		}

		//8192 hz, 262144 hz
		int[] transferFrequencyCycles = {512, 16};

		//TODO:
		//-actual link cable support?
		//-external clock support (clock pin)
		public void Step(){
			byte sb = memory.regs.SB;

			int transferEnable = memory.regs.SIO_EN;
			int clockSelect = memory.regs.SIO_CLK;
			int clockSpeed = memory.regs.SIO_FAST;

			bool clockTicked = false;

			if(clockSelect == 1){
				cycleCount++;

				if(cycleCount >= transferFrequencyCycles[0]){
					cycleCount = 0;
					clockTicked = true;
				}
			}

			//If the internal/external clock ticked, check whether we need to transfer a bit
			if(clockTicked && doingTransfer){
				/*
				For now, serial transfer stuff is just faked, acting as if
				a link cable is never inserted (outputted bits are ignored,
				recieved bits are 1). Internal clock is assumed.
				*/
				memory.regs.SB = TransferBit(sb);
				shiftCount--;
				//Are there any more bits to shift in/out?
				if(shiftCount == 0){
					//If not, clear bit 7 of SC, and request a serial interrupt
					memory.regs.SIO_EN = 0;
					memory.RequestInterrupt(Interrupt.Serial);
					doingTransfer = false;
				}
			}
		}

		byte TransferBit(byte sb){
			int bitToSend = (sb >> 7) & 1;
			sb <<= 1;

			//For now, the Game Boy always just recieves 1 bits,
			//as if a link cable isn't connected.
			int recievedBit = 1;
			sb |= (byte)recievedBit;

			return sb;
		}

	}
}
