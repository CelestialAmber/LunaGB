using System;
namespace LunaGB.Core {
	public class CPU {

		public byte A, B, C, D, E, F, H, L; //8-bit registers
		public ushort sp, pc; //stack pointer, program counter

		/*
        condition flags
        stored in the f register

        Bits (lower 4 are always 0):
        Z N H C 0 0 0 0
        7 6 5 4 3 2 1 0
        */

		public int flagZ {
			get {
				return GetFlag(3);
			}
			set {
				SetFlag(3, value);
			}
		}

		public int flagN {
			get {
				return GetFlag(2);
			}
			set {
				SetFlag(2, value);
			}
		}

		public int flagH {
			get {
				return GetFlag(1);
			}
			set {
				SetFlag(1, value);
			}
		}

		public int flagC {
			get {
				return GetFlag(0);
			}
			set {
				SetFlag(0, value);
			}
		}


		public int cycles; //current number of cycles (not divided by 4 for now)
		public bool lowPowerMode; //low power mode flag
		public bool gbcCpuSpeed; //false: regular cpu speed (gb), true: 2x cpu speed (gbc)
		public bool interruptsEnabled; //are interrupts currently enabled?

		//Register pairs
		public ushort AF {
			get {
				return (ushort)((A << 8) + F);
			}
			set {
				A = (byte)(value >> 8);
				F = (byte)value;
			}
		}

		public ushort BC {
			get {
				return (ushort)((B << 8) + C);
			}
			set {
				B = (byte)(value >> 8);
				C = (byte)value;
			}
		}

		public ushort DE {
			get {
				return (ushort)((D << 8) + E);
			}
			set {
				D = (byte)(value >> 8);
				E = (byte)value;
			}
		}

		public ushort HL {
			get {
				return (ushort)((H << 8) + L);
			}
			set {
				H = (byte)(value >> 8);
				L = (byte)value;
			}
		}

		//Used to keep track of if a daa instruction comes after a subtract function
		bool lastInstructionWasSubtract = false;

		Memory memory;




		public CPU(Memory memory) {
			this.memory = memory;
		}

		public void ExecuteInstruction() {
			byte opcode = ReadByte();
			byte lo = opcode & 0xF;
			byte hi = opcode >> 4;

			if(opcode < 0x40){
				switch(lo){
					case 0x00:
						if(opcode == 0x00){
							//nop (4 cycles)
							cycles += 4;
						}else if(opcode == 0x10){
							//enter low power mode, also used to switch between gbc and gb cpu modes
							//not sure how this behaves
							lowPowerMode = true;
						}else if(opcode == 0x20){
							//jr nz,n8
							//relative jump if zero flag not set
							//the offset is an 8 bit signed number (-128 to 128)

							jumpOffset = (sbyte)ReadByte();

                    		if(flagZ == 0) {
								pc = (ushort)(pc + jumpOffset);
								cycles += 12; //If the branch is taken, the instruction takes an extra 4 cycles
							} else cycles += 8; //If not, it only takes 8
						}else if(opcode == 0x30){
							//jr nc,n8
							//relative jump if carry not set
							//the offset is an 8 bit signed number (-128 to 128)

							jumpOffset = (sbyte)ReadByte();

                    		if(flagC == 0) {
								pc = (ushort)(pc + jumpOffset);
								cycles += 12; //If the branch is taken, the instruction takes an extra 4 cycles
							} else cycles += 8; //If not, it only takes 8
						}
						break;
					case 0x01:
						//ld r16,n16
						ushort val = ReadUInt16();
					
						if(hi == 0x00) BC == val;
						else if(hi == 0x01) DE == val;
						else if(hi == 0x02) HL == val;
						else sp == val;
						cycles += 12;
						break;
					case 0x02:
						//ld (r16),a
						if(hi == 0x00)memory.WriteByte(BC, A);
						else if(hi == 0x01)memory.WriteByte(DE, A);
						else if(hi == 0x02){
							memory.WriteByte(HL, A);
							HL++;
						}
						else{
							memory.WriteByte(HL, A);
							HL--;
						}
						cycles += 8;
						break;
					case 0x03:
						//inc r16
						if(hi == 0x00) BC++; //0x03
						else if(hi == 0x01) DE++; //0x13
						else if(hi == 0x02) HL++; //0x23
						else sp++; //0x33
						cycles += 8;
						break;
					case 0x04:
					case 0x0C:
						//inc r8/inc (hl)

						byte result = 0;

						if(opcode == 0x04){
							B++;
							result = B;
						}
						else if(opcode == 0x0C){
							C++;
							result = C;
						}
						else if(opcode == 0x14){
							D++;
							result = D;
						}
						else if(opcode == 0x1C){
							E++;
							result = E;
						}
						else if(opcode == 0x24){
							H++;
							result = H;
						}
						else if(opcode == 0x2C){
							L++;
							result = L;
						}
						else if(opcode == 0x34){
							//inc (hl)
							byte temp = (byte)(memory.GetByte(HL) + 1);
							memory.WriteByte(HL, temp);
							result = temp;
							cycles += 8; //takes an additional 8 cycles;
						}
						else if(opcode == 0x3C){
							A++;
							result = A;
						}

						flagN = 0;
						flagZ = temp == 0 ? 1 : 0;
						flagH = (temp & 0xF) == 0 ? 1 : 0; //If the lower nibble is 0, a carry occured from bit 3 to bit 4
						cycles += 4;
						break;
					case 0x05:
					case 0x0D:
						//dec r8/dec (hl)

						result = 0;

						if(opcode == 0x05){
							B--;
							result = B;
						}
						else if(opcode == 0x0D){
							C--;
							result = C;
						}
						else if(opcode == 0x15){
							D--;
							result = D;
						}
						else if(opcode == 0x1D){
							E--;
							result = E;
						}
						else if(opcode == 0x25){
							H--;
							result = H;
						}
						else if(opcode == 0x2D){
							L--;
							result = L;
						}
						else if(opcode == 0x35){
							//dec (hl)
							byte temp = (byte)(memory.GetByte(HL) - 1);
							memory.WriteByte(HL, temp);
							result = temp;
							cycles += 8; //takes an additional 8 cycles;
						}
						else if(opcode == 0x3D){
							A--;
							result = A;
						}

						flagN = 1;
						flagZ = temp == 0 ? 1 : 0;
						flagH = (temp & 0xF) == 0 ? 1 : 0; //If the lower nibble is FF, a borrow occured from bit 3 to bit 4
						cycles += 4;
						break;
					case 0x06:
					case 0x0E:
						//ld r8,n8/ld (hl),n8
						byte val = ReadByte();

						if(opcode == 0x06) B = val;
						else if(opcode == 0x0E) C = val;
						else if(opcode == 0x16) D = val;
						else if(opcode == 0x1E) E = val;
						else if(opcode == 0x26) H = val;
						else if(opcode == 0x2E) L = val;
						else if(opcode == 0x36){
							//ld (hl),n8
							memory.WriteByte(HL, val);
							cycles += 4; //Takes an additional 4 cycles
						}
						else if(opcode == 0x3E) A = val;
						cycles += 8;
						break;
					case 0x07:
						if(hi == 0x00){ //0x07
							//rlca
							int carryBit = A >> 7;
							//rotate a left
							A = (byte)(((A << 1) & 0xFF) + (A >> 7));
							//set carry flag
							flagC = carryBit;
							//reset H/Z/N flags
							flagN = 0;
							flagZ = 0;
							flagH = 0;
							cycles += 4;
						}else if(hi == 0x01){ //0x17
							//rla
							//rotate a left, including the carry flag
							//C  1 2 3 4 5 6 7 8 -> 1  2 3 4 5 6 7 8 C
							carryBit = flagC;
							int newCarryBit = A >> 7;
							A = (byte)(((A << 1) & 0xFF) + carryBit);
							//set carry flag
							flagC = newCarryBit;
							//reset H/Z/N flags
							flagN = 0;
							flagZ = 0;
							flagH = 0;
							cycles += 4;
						}else if(hi == 0x02){ //0x27
							//daa
							//Adjusts a (sum of two BCD numbers from previous add/sub instruction) to the right BCD

							int n = 0; //amount to adjust a by

							//Add 6 to either digit to correct them if needed
							if(flagH == 1 || (A & 0xF) > 9) {
								n = 6;
                   			}
							if(flagC == 1 || A > 0x99) {
								n += 0x60;
							}

							if(lastInstructionWasSubtract) A = (byte)(A - n);
							else A = (byte)(A + n);

							flagC = A > 0x99 ? 1 : 0;
							flagZ = A == 0 ? 1 : 0;
							flagH = 0;

							cycles += 4;
						}else if(hi == 0x03){ //0x37
							//scf
							flagC = 1;
							flagN = 0;
							flagH = 0;
							cycles += 4;
						}
						break;
					case 0x08:
						if(hi == 0x00){ //0x08
							//ld (n16),sp
							ushort address = ReadUInt16();
							memory.WriteByte(address, (byte)(sp & 0xFF)); //set the value at the address to sp & 0xFF
							cycles += 20;
						}else if(hi == 0x01){ //0x18
							//jr n8
							//relative jump
							//the offset is an 8 bit signed number (-128 to 128)

							byte jumpOffset = (sbyte)ReadByte();
							pc = (ushort)(pc + jumpOffset);
							cycles += 12; //If the branch is taken, the instruction takes an extra 4 cycles
						}else if(hi == 0x02){ //0x28
							//jr z,n8
							//relative jump if zero flag set
							//the offset is an 8 bit signed number (-128 to 128)

							byte jumpOffset = (sbyte)ReadByte();

                    		if(flagZ == 1) {
								pc = (ushort)(pc + jumpOffset);
								cycles += 12; //If the branch is taken, the instruction takes an extra 4 cycles
							} else cycles += 8; //If not, it only takes 8
						}else if(hi == 0x02){ //0x38
							//jr c,r8
							//relative jump if carry set
							//the offset is an 8 bit signed number (-128 to 128)

							byte jumpOffset = (sbyte)ReadByte();

                    		if(flagC == 1) {
								pc = (ushort)(pc + jumpOffset);
								cycles += 12; //If the branch is taken, the instruction takes an extra 4 cycles
							} else cycles += 8; //If not, it only takes 8
						}
						break;
					case 0x09:
						ushort val = hi == 0x00 ? BC : hi == 0x01 ? DE : hi == 0x02 ? HL : sp;
						flagH = (HL & 0xFFF) + val > 0xFFF ? 1 : 0; //Set if overflow from bit 11
						flagC = HL + val > 0xFFFF;
						HL += val;
						flagN = 0;
						cycles += 8;
						break;
					case 0x0A:
						//ld a,(r16)
						ushort val = ReadUInt16();
					
						if(hi == 0x00)A = memory.ReadByte(BC); //0x0A
						else if(hi == 0x01)A = memory.ReadByte(DE); //0x1A
						else if(hi == 0x02){
							//0x2A
							A = memory.ReadByte(HL);
							HL++;
						}
						else{
							//0x3A
							A = memory.ReadByte(HL);
							HL--;
						}
						cycles += 8;
						break;
					case 0x0B:
						//dec r16
						if(hi == 0x00) BC--; //0x0B
						else if(hi == 0x01) DE--; //0x1B
						else if(hi == 0x02) HL--; //0x2B
						else sp--; //0x3B
						cycles += 8;
						break;
					case 0x0F:
						if(hi == 0x00){ //0x0F
							//rrca
							carryBit = A & 1;
							//rotate a right
							A = (byte)((A >> 1) + (A << 7) & 0xFF);
							//set carry
							flagC = carryBit;
							//reset H/Z/N flags
							flagN = 0;
							flagZ = 0;
							flagH = 0;
							cycles += 4;
						}else if(hi == 0x01){ //0x1F
							//rra
							//rotate a right, including the carry flag
							//C  1 2 3 4 5 6 7 8 -> 8  C 1 2 3 4 5 6 7
							carryBit = flagC;
							newCarryBit = A & 1;
							A = (byte)((A >> 1) + (carryBit << 7));
							//set carry flag
							flagC = newCarryBit;
							//reset H/Z/N flags
							flagN = 0;
							flagZ = 0;
							flagH = 0;
							cycles += 4;
						}else if(hi == 0x02){ //0x2F
							//cpl
							A = (byte)(~A);
							flagN = 1;
							flagH = 1;
							cycles += 4;
						}else if(hi == 0x03){ //0x3F
							//ccf
							flagC = 1 - flagC;
							flagN = 0;
							flagH = 0;
							cycles += 4;
						}
						break;
				}
			}else if(opcode < 0xC0){
				if(opcode < 0x80){
					//The halt instruction takes the place of ld (hl),(hl)
					if(opcode == 0x76){
						//halt
					}else{
						//main load instructions
						Ld8(opcode);
					}
				}else{
					//math instructions	
					if(opcode < 0x90){ //add/adc (0x80-8F)
						if(opcode < 0x88){
							Add(opcode);
						}else{
							Adc(opcode);
						}
					}else if(opcode < 0xA0){ //sub/sbc (0x90-9F)
						if(opcode < 0x98){
							Sub(opcode);
						}else{
							Sbc(opcode);
						}
					}else if(opcode < 0xB0){ //and/xor (0xA0-AF)
						if(opcode < 0xA8){
							And(opcode);
						}else{
							Xor(opcode);
						}
					}else if(opcode < 0xC0){ //or/cp (0xB0-BF)
						if(opcode < 0xB8){
							Or(opcode);
						}else{
							Cp(opcode);
						}
					}
				}
			}else{
				//opcodes 0xC0-0xFF
			switch(opcode) {
				case 0xC0:
					//ret nz
					if(flagZ == 0) {
						pc = Pop();
						cycles += 20;
					} else cycles += 8;
					break;
				case 0xC1:
					//pop bc (12 cycles)
					BC = Pop();
					cycles += 12;
					break;
				case 0xC2:
					//jp nz,n16
					ushort address = ReadUInt16();

					if(flagZ == 0){
						pc = address;
						cycles += 24;
					}else cycles += 12;
					break;
				case 0xC3:
					//jp n16
					ushort address = ReadUInt16();
					pc = address;
					cycles += 16;
					break;
				case 0xC4:
					//call nz,n16
					ushort address = ReadUInt16();

					if(flagZ == 0){
						Call(address);
						cycles += 24;
					}else cycles += 12;
					break;
				case 0xC5:
					//push bc (16 cycles)
					Push(BC);
					cycles += 16;
					break;
				case 0xC6:
					//add a,n8
					byte val = ReadByte();
					CheckCarry();
					A += val;
					flagN = 0;
					flagZ = A == 0 ? 1 : 0;
					cycles += 8;
					break;
				case 0xC7:
					//rst 0x00
					//Jump to reset vector 0x00
					RST(0x00);
					break;
				case 0xC8:
					//ret z
					if(flagZ == 1) {
						pc = Pop();
						cycles += 20;
					} else cycles += 8;

					break;
				case 0xC9:
					//ret
					Return();
					cycles += 16;
					break;
				case 0xCA:
					//jp z,n16
					ushort address = ReadUInt16();

					if(flagZ == 1){
						pc = address;
						cycles += 24;
					}else cycles += 12;
					break;
				case 0xCB:
					//CB prefix instructions (bit instructions)
					//handle all cb instructions here
					ExecuteCBInstruction();
					break;
				case 0xCC:
					//call z,n16
					ushort address = ReadUInt16();

					if(flagZ == 1){
						Call(address);
						cycles += 24;
					}else cycles += 12;
					break;
				case 0xCD:
					//call n16
					ushort address = ReadUInt16();

					Call(address);
					cycles += 24;
					break;
				case 0xCE:
					//adc a,d8
					byte val = ReadByte();
					int carry = flagC;
					CheckCarry();
					A += val + carry;
					flagN = 0;
					flagZ = A == 0 ? 1 : 0;
					cycles += 8;
					break;
				case 0xCF:
					//rst 0x8
					//Jump to reset vector 0x8
					RST(0x8);
					break;
				case 0xD0:
					//ret nc
					if(flagC == 0) {
						pc = Pop();
						cycles += 20;
					} else cycles += 8;
					break;
				case 0xD1:
					//pop de (12 cycles)
					DE = Pop();
					cycles += 12;
					break;
				case 0xD2:
					//jp nc,n16
					ushort address = ReadUInt16();

					if(flagC == 0){
						pc = address;
						cycles += 24;
					}else cycles += 12;
					break;
				case 0xD4:
					//call nc,n16
					ushort address = ReadUInt16();

					if(flagC == 0){
						Call(address);
						cycles += 24;
					}else cycles += 12;
					break;
				case 0xD5:
					//push de (16 cycles)
					Push(DE);
					cycles += 16;
					break;
				case 0xD6:
					//sub n8
					byte val = ReadByte();
					CheckBorrow();
					A -= val;
					flagN = 1;
					flagZ = A == 0 ? 1 : 0;
					cycles += 8;
					break;
				case 0xD7:
					//rst 0x10
					//Jump to reset vector 0x10
					RST(0x10);
					break;
				case 0xD8:
					//ret c
					if(flagC == 1) {
						pc = Pop();
						cycles += 20;
					} else cycles += 8;
					break;
				case 0xD9:
					//reti
					Return();
					interruptsEnabled = true;
					cycles += 16;
					break;
				case 0xDA:
					//jp c,n16
					ushort address = ReadUInt16();

					if(flagC == 1){
						pc = address;
						cycles += 24;
					}else cycles += 12;
					break;
				case 0xDC:
					//call c,n16
					ushort address = ReadUInt16();

					if(flagC == 1){
						Call(address);
						cycles += 24;
					}else cycles += 12;
					break;
				case 0xDE:
					//sbc a,d8
					byte val = ReadByte();
					int carry = flagC;
					CheckBorrow();
					A -= val + carry;
					flagN = 1;
					flagZ = A == 0 ? 1 : 0;
					cycles += 8;
					break;
				case 0xDF:
					//rst 0x18
					//Jump to reset vector 0x18
					RST(0x18);
					break;
				case 0xE0:
					//ldh (n8),a
					byte offset = ReadByte();
					memory.WriteByte(0xFF00 + offset,A);
					cycles += 12;
					break;
				case 0xE1:
					//pop hl (12 cycles)
					HL = Pop();
					cycles += 12;
					break;
				case 0xE2:
					//ldh (c),a
					memory.WriteByte(0xFF00 + C,A);
					cycles += 8;
					break;
				case 0xE5:
					//push hl (16 cycles)
					Push(HL);
					cycles += 16;
					break;
				case 0xE6:
					//and n8
					byte val = ReadByte();
					A &= val;
					flagZ = A == 0 ? 1 : 0;
					flagN = 0;
					flagH = 1;
					flagC = 0;
					cycles += 8;
					break;
				case 0xE7:
					//rst 0x20
					//Jump to reset vector 0x20
					RST(0x20);
					break;
				case 0xE8:
					//add sp,n8
					byte val = ReadByte();
					CheckCarry(sp,val);
					sp += val;
					cycles += 16;
					break;
				case 0xE9:
					//jp (hl)
					pc = memory.ReadByte(HL);
					cycles += 4;
					break;
				case 0xEA:
					//ld (n16),a
					ushort address = ReadUInt16();
					memory.WriteByte(address,a);
					cycles += 16;
					break;
				case 0xEE:
					//xor n8
					byte val = ReadByte();
					A ^= val;
					flagZ = A == 0 ? 1 : 0;
					flagN = 0;
					flagH = 0;
					flagC = 0;
					cycles += 8;
					break;
				case 0xEF:
					//rst 0x28
					//Jump to reset vector 0x28
					RST(0x28);
					break;
				case 0xF0:
					//ldh a,(n8)
					byte offset = ReadByte();
					A = memory.ReadByte(0xFF00 + offset);
					cycles += 12;
					break;
				case 0xF1:
					//pop af (12 cycles)
					AF = Pop();
					cycles += 12;
					break;
				case 0xF2:
					//ldh a,(c)
					A = memory.ReadByte(0xFF00 + C);
					cycles += 8;
					break;
				case 0xF3:
					//di (4 cycles)
					//Disable interrupts
					interruptsEnabled = false;
					cycles += 4;
					break;
				case 0xF5:
					//push af
					Push(AF);
					cycles += 16;
					break;
				case 0xF6:
					//or n8
					byte val = ReadByte();
					A |= val;
					flagZ = A == 0 ? 1 : 0;
					flagN = 0;
					flagH = 0;
					flagC = 0;
					cycles += 8;
					break;
				case 0xF7:
					//rst 0x30
					//Jump to reset vector 0x30
					RST(0x30);
					break;
				case 0xF8:
					//ld hl,sp+n8
					byte offset = ReadByte();
					CheckCarry(sp, offset);
					hl = sp + offset;
					cycles += 8;
					break;
				case 0xF9:
					//ld sp,hl
					sp = HL;
					cycles += 8;
					break;
				case 0xFA:
					//ld a,(n16)
					ushort address = ReadUInt16();
					a = memory.ReadByte(address);
					cycles += 16;
					break;
				case 0xFB:
					//ei
					//Enable interrupts
					interruptsEnabled = true;
					cycles += 4;
					break;
				case 0xFE:
					//cp n8
					byte val = ReadByte();
					Compare(val);
					cycles += 8;
					break;
				case 0xFF:
					//rst 0x38
					//Jump to reset vector 0x38
					RST(0x38);
					break;
				default:
					throw new NotImplementedException("Instruction not implemented. It should be though?");
			}
			}
		}

		void ExecuteCBInstruction(){

		}


		//The lower nibble of the opcode encodes which register to use
		//If the lower nibble is 6, the instruction uses (hl)
		//The bitwise/math functions only cover the versions with register params (opcodes 8x-Bx)
		//All instructions of this type take 8 cycles (add n/others take 4)

		void Xor(byte opcode) {
			int regIndex = (opcode & 0xF) - 8;
			byte val = regIndex == 6 ? memory.GetByte(HL) : GetRegisterVal(opcode & 0xF);
			A ^= val;
			flagZ = A == 0 ? 1 : 0;
			flagN = 0;
			flagH = 0;
			flagC = 0;
			cycles += regIndex == 6 ? 8 : 4;
		}

		void And(byte opcode) {
			int regIndex = opcode & 0xF;
			byte val = regIndex == 6 ? memory.GetByte(HL) : GetRegisterVal(opcode & 0xF);
			A &= val;
			flagZ = A == 0 ? 1 : 0;
			flagN = 0;
			flagC = 0;
			flagH = 1;
			cycles += regIndex == 6 ? 8 : 4;
		}

		void Sub(byte opcode) {
			int regIndex = opcode & 0xF;
			byte val = regIndex == 6 ? memory.GetByte(HL) : GetRegisterVal(opcode & 0xF);
			CheckBorrow(A,val);
			A -= val;
			lastInstructionWasSubtract = true;
			flagZ = A == 0 ? 1 : 0;
			flagN = 1;
			cycles += regIndex == 6 ? 8 : 4;
		}

		void Add(byte opcode) {
			int regIndex = opcode & 0xF;
			byte val = regIndex == 6 ? memory.GetByte(HL) : GetRegisterVal(opcode & 0xF);
			CheckCarry(A,val);
			A += val;
			flagZ = A == 0 ? 1 : 0;
			flagN = 0;
			cycles += regIndex == 6 ? 8 : 4;
		}

		void Or(byte opcode) {
			int regIndex = opcode & 0xF;
			byte val = regIndex == 6 ? memory.GetByte(HL) : GetRegisterVal(opcode & 0xF);
			A ^= val;
			flagZ = A == 0 ? 1 : 0;
			flagC = 0;
			flagH = 0;
			flagN = 0;
			cycles += regIndex == 6 ? 8 : 4;
		}

		void Adc(byte opcode) {
			int regIndex = (opcode & 0xF) - 8;
			byte val = regIndex == 6 ? memory.GetByte(HL) : GetRegisterVal(opcode & 0xF);
			int carry = flagC;
			CheckCarry();
			A += val + carry;
			flagZ = A == 0 ? 1 : 0;
			cycles += regIndex == 6 ? 8 : 4;
		}

		void Sbc(byte opcode) {
			int regIndex = (opcode & 0xF) - 8;
			byte val = regIndex == 6 ? memory.GetByte(HL) : GetRegisterVal(opcode & 0xF);
			int carry = flagC;
			CheckBorrow();
			A -= val + carry;
			lastInstructionWasSubtract = true;
			flagZ = A == 0 ? 1 : 0;
			cycles += regIndex == 6 ? 8 : 4;
		}

		void Cp(byte opcode) {
			int regIndex = (opcode & 0xF) - 8;
			byte val = regIndex == 6 ? memory.GetByte(HL) : GetRegisterVal(opcode & 0xF);
			Compare(val);
			cycles += regIndex == 6 ? 8 : 4;
		}



		void Compare(byte val){
			flagZ = A == val ? 1 : 0;
			flagN = 1;
			CheckBorrow(A,val);
		}

		void CheckCarry(byte a, byte b){
			//Check for half carry and full carry
			flagH = (a & 0xF) + (b & 0xF) > 0xF ? 1 : 0;
			flagC = a + b > 0xFF ? 1 : 0;
		}

		void CheckBorrow(byte a, byte b){
			//Check for half borrow and full borrow
			flagH = (a & 0xF) - (b & 0xF) < 0 ? 1 : 0;
			flagC = a - b < 0 ? 1 : 0;
		}

		//Used for all 8bit loads instructions except the 0x-3x ones
		void Ld8(byte opcode) {
			//calculate the index of the set register
			//The opcodes are organised like this for set values:
			//0x40: B, 0x48: C, 0x50: D, etc...
			//index = low nibble/8 + (high nibble - 4)/16
			byte lo = (byte)(opcode & 0xF), hi = (byte)(opcode >> 4);
			int setRegIndex = lo / 8 + (hi - 0x4) / 16;
			int loadRegIndex = lo;

			//Load the value from the source register to the destination register
			byte val = loadRegIndex == 6 ? memory.GetByte(HL) : GetRegisterVal(opcode & 0xF);
			SetRegister(setRegIndex, val);
		}

		//Calls the given function by pushing the address of the next instruction, then jumping to the called function.
		void Call(ushort address){
			Push(pc); //push the address of the next instruction to the stack for later
			pc = address; //Jump to the function
		}

		//Returns from a function back to the caller function.
		void Return(){
			pc = Pop();
		}


		//Gets the value of a specified register (using the lower opcode nibble)
		//Used for instructions like add (8 bit registers or add (hl)) or ld
		//instructions using (hl) (index 6) are handled in the separate functions so that values can just be bytes
		byte GetRegisterVal(int regIndex) {
			switch(regIndex) {
				case 0:
					return B;
				case 1:
					return C;
				case 2:
					return D;
				case 3:
					return E;
				case 4:
					return H;
				case 5:
					return L;
				case 7:
					return A;
				default:
					throw new IndexOutOfRangeException("Invalid register index");
			}
		}

		//Sets the specified register to the given value
		//Used for load instructions
		void SetRegister(int regIndex, byte val) {
			switch(regIndex) {
				case 0:
					B = val;
					break;
				case 1:
					C = val;
					break;
				case 2:
					D = val;
					break;
				case 3:
					E = val;
					break;
				case 4:
					H = val;
					break;
				case 5:
					L = val;
					break;
				case 6:
					memory.WriteByte(HL, val); //set the value at address hl
					break;
				case 7:
					A = val;
					break;
				default:
					throw new IndexOutOfRangeException("Invalid register index");
			}
		}

		//Sets the given flag to the given value
		void SetFlag(int index, int val) {
			F = (byte)((F & ~(1 << (index + 4))) | (val << (index + 4)));
		}

		//Gets the requested flag
		int GetFlag(int index) {
			return (F >> (index + 4)) & 1;
		}


		byte ReadByte() {
			byte b = memory.GetByte(pc);
			pc++;
			return b;
		}

		ushort ReadUInt16() {
			ushort val = memory.GetUInt16(pc);
			pc += 2;
			return val;
		}

		ushort Pop() {
			//get the 16bit value at the top of the stack, and increment the stack pointer by 2
			ushort val = memory.GetUInt16(sp);
			sp += 2;
			return val;
        }

		void Push(ushort val) {
			//Decrease the stack pointer by 2 and push the value onto the stack
			sp -= 2;
			memory.WriteUInt16(sp, val);
        }

		//Calls a reset vector.
		void RST(byte vector) {
			pc = vector;
			cycles += 16;
        }
	}
}
