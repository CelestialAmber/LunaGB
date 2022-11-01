using System;
namespace LunaGB.Core
{
	public class CPU {

		public byte A, B, C, D, E, F, H, L; //8-bit registers
        public ushort sp, pc; //stack pointer, program counter
        public bool flagZ, flagNZ, flagC, flagNC; //condition flags
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


        Memory memory;


        public CPU(Memory memory){
            this.memory = memory;
		}


		public void ExecuteInstruction() {
            byte opcode = ReadByte();

            switch (opcode) {
                case 0x00:
                    //nop (4 cycles)
                    cycles += 4;
                    break;
                case 0x01:
                    //ld bc,n16 (12 cycles)
                    BC = ReadUInt16();
                    cycles += 12;
                    break;
                case 0x02:
                    //ld (bc), a (8 cycles)
                    memory.WriteByte(BC, A);
                    cycles += 8;
                    break;
                case 0x03:
                    //inc bc (8 cycles)
                    BC++;
                    cycles += 8;
                    break;
                case 0x04:
                    //inc b (4 cycles)
                    B++;
                    cycles += 4;
                    break;
                case 0x05:
                    //dec b (4 cycles)
                    B--;
                    cycles += 4;
                    break;
                case 0x06:
                    //ld b,n8 (8 cycles)
                    B = ReadByte();
                    cycles += 8;
                    break;
                case 0x07:
                    //rlca (4 cycles)
                    int carryBit = A >> 7;
                    //rotate a left
                    A = (byte)(((A << 1) & 0xFF) + (A >> 7));
                    //set carry flag
                    SetFlag(0, carryBit);
                    //reset H/Z/N flags
                    SetFlag(1, 0);
                    SetFlag(2, 0);
                    SetFlag(3, 0);
                    cycles += 4;
                    break;
                case 0x08:
                    //ld (n16),sp (20 cycles)
                    ushort address = ReadUInt16();
                    memory.WriteByte(address, (byte)(sp & 0xFF)); //set the value at the address to sp & 0xFF
                    cycles += 20;
                    break;
                case 0x09:
                    //add hl,bc (8 cycles)
                    HL += BC;
                    cycles += 8;
                    break;
                case 0x0A:
                    break;
                case 0x0B:
                    //dec bc (8 cycles)
                    BC--;
                    cycles += 8;
                    break;
                case 0x0C:
                    //inc c (4 cycles)
                    C++;
                    cycles += 4;
                    break;
                case 0x0D:
                    //dec c (4 cycles)
                    C--;
                    cycles += 4;
                    break;
                case 0x0E:
                    //ld c,n8 (8 cycles)
                    C = ReadByte();
                    cycles += 8;
                    break;
                case 0x0F:
                    //rrca (4 cycles)
                    carryBit = A & 1;
                    //rotate a right
                    A = (byte)((A >> 1) + (A << 7) & 0xFF);
                    //set carry
                    SetFlag(0, carryBit);
                    //reset H/Z/N flags
                    SetFlag(1, 0);
                    SetFlag(2, 0);
                    SetFlag(3, 0);
                    cycles += 4;
                    break;
                case 0x10:
                    //stop
                    //enter low power mode, also used to switch between gbc and gb cpu modes
                    //not sure how this behaves
                    lowPowerMode = true;
                    break;
                case 0x11:
                    //ld de,n16 (12 cycles)
                    DE = ReadUInt16();
                    cycles += 12;
                    break;
                case 0x12:
                    //ld (de), a (8 cycles)
                    memory.WriteByte(DE, A);
                    cycles += 8;
                    break;
                case 0x13:
                    //inc de (8 cycles)
                    DE++;
                    cycles += 8;
                    break;
                case 0x14:
                    //inc d (4 cycles)
                    D++;
                    cycles += 4;
                    break;
                case 0x15:
                    //dec d (4 cycles)
                    D--;
                    cycles += 4;
                    break;
                case 0x16:
                    //ld d,n8 (8 cycles)
                    D = ReadByte();
                    cycles += 8;
                    break;
                case 0x17:
                    //rla (4 cycles)
                    //rotate a left, including the carry flag
                    //C  1 2 3 4 5 6 7 8 -> 1  2 3 4 5 6 7 8 C
                    carryBit = GetFlag(4);
                    int newCarryBit = A >> 7;
                    A = (byte)(((A << 1) & 0xFF) + carryBit);
                    //set carry flag
                    SetFlag(0, newCarryBit);
                    //reset H/Z/N flags
                    SetFlag(1, 0);
                    SetFlag(2, 0);
                    SetFlag(3, 0);
                    cycles += 4;
                    break;
                case 0x18:
                    //jr r8 (12 cycles)
                    //Relative jump
                    //TODO: check from what address the offset is from (start of current/next instruction)
                    byte offset = ReadByte();
                    pc += offset;
                    cycles += 12;
                    break;
                case 0x19:
                    //add hl,de (8 cycles)
                    HL += DE;
                    cycles += 8;
                    break;
                case 0x1A:
                    break;
                case 0x1B:
                    //dec de (8 cycles)
                    DE--;
                    cycles += 8;
                    break;
                case 0x1C:
                    //inc e (4 cycles)
                    E++;
                    cycles += 4;
                    break;
                case 0x1D:
                    //dec e (4 cycles)
                    E--;
                    cycles += 4;
                    break;
                case 0x1E:
                    break;
                case 0x1F:
                    //rra (4 cycles)
                    //rotate a right, including the carry flag
                    //C  1 2 3 4 5 6 7 8 -> 8  C 1 2 3 4 5 6 7
                    carryBit = GetFlag(4);
                    newCarryBit = A & 1;
                    A = (byte)((A >> 1) + (carryBit << 7));
                    //set carry flag
                    SetFlag(0, newCarryBit);
                    //reset H/Z/N flags
                    SetFlag(1, 0);
                    SetFlag(2, 0);
                    SetFlag(3, 0);
                    cycles += 4;
                    break;
                case 0x20:
                    break;
                case 0x21:
                    //ld hl,n16 (12 cycles)
                    HL = ReadUInt16();
                    cycles += 12;
                    break;
                case 0x22:
                    break;
                case 0x23:
                    //inc hl (8 cycles)
                    HL++;
                    cycles += 8;
                    break;
                case 0x24:
                    //inc h (4 cycles)
                    H++;
                    cycles += 4;
                    break;
                case 0x25:
                    //dec h (4 cycles)
                    H--;
                    cycles += 4;
                    break;
                case 0x26:
                    //ld h,n8 (8 cycles)
                    H = ReadByte();
                    cycles += 8;
                    break;
                case 0x27:
                    break;
                case 0x28:
                    break;
                case 0x29:
                    //add hl,hl (8 cycles)
                    HL += HL;
                    cycles += 8;
                    break;
                case 0x2A:
                    break;
                case 0x2B:
                    //dec hl (8 cycles)
                    HL--;
                    cycles += 8;
                    break;
                case 0x2C:
                    //inc l (4 cycles)
                    L++;
                    cycles += 4;
                    break;
                case 0x2D:
                    //dec l (4 cycles)
                    L--;
                    cycles += 4;
                    break;
                case 0x2E:
                    break;
                case 0x2F:
                    break;
                case 0x30:
                    break;
                case 0x31:
                    //ld sp,n16 (12 cycles)
                    sp = ReadUInt16();
                    cycles += 12;
                    break;
                case 0x32:
                    break;
                case 0x33:
                    //inc sp (8 cycles)
                    sp++;
                    cycles += 8;
                    break;
                case 0x34:
                    break;
                case 0x35:
                    break;
                case 0x36:
                    //ld (hl),n8 (12 cycles)
                    memory.WriteByte(HL, ReadByte());
                    cycles += 12;
                    break;
                case 0x37:
                    break;
                case 0x38:
                    break;
                case 0x39:
                    //add hl,sp (8 cycles)
                    HL += sp;
                    cycles += 8;
                    break;
                case 0x3A:
                    break;
                case 0x3B:
                    //dec sp (8 cycles)
                    sp--;
                    cycles += 8;
                    break;
                case 0x3C:
                    //inc a (4 cycles)
                    A++;
                    cycles += 4;
                    break;
                case 0x3D:
                    //dec a (4 cycles)
                    A--;
                    cycles += 4;
                    break;
                case 0x3E:
                    break;
                case 0x3F:
                    break;
                case 0x40:
                    break;
                case 0x41:
                    break;
                case 0x42:
                    break;
                case 0x43:
                    break;
                case 0x44:
                    break;
                case 0x45:
                    break;
                case 0x46:
                    break;
                case 0x47:
                    break;
                case 0x48:
                    break;
                case 0x49:
                    break;
                case 0x4A:
                    break;
                case 0x4B:
                    break;
                case 0x4C:
                    break;
                case 0x4D:
                    break;
                case 0x4E:
                    break;
                case 0x4F:
                    break;
                case 0x50:
                    break;
                case 0x51:
                    break;
                case 0x52:
                    break;
                case 0x53:
                    break;
                case 0x54:
                    break;
                case 0x55:
                    break;
                case 0x56:
                    break;
                case 0x57:
                    break;
                case 0x58:
                    break;
                case 0x59:
                    break;
                case 0x5A:
                    break;
                case 0x5B:
                    break;
                case 0x5C:
                    break;
                case 0x5D:
                    break;
                case 0x5E:
                    break;
                case 0x5F:
                    break;
                case 0x60:
                    break;
                case 0x61:
                    break;
                case 0x62:
                    break;
                case 0x63:
                    break;
                case 0x64:
                    break;
                case 0x65:
                    break;
                case 0x66:
                    break;
                case 0x67:
                    break;
                case 0x68:
                    break;
                case 0x69:
                    break;
                case 0x6A:
                    break;
                case 0x6B:
                    break;
                case 0x6C:
                    break;
                case 0x6D:
                    break;
                case 0x6E:
                    break;
                case 0x6F:
                    break;
                case 0x70:
                    break;
                case 0x71:
                    break;
                case 0x72:
                    break;
                case 0x73:
                    break;
                case 0x74:
                    break;
                case 0x75:
                    break;
                case 0x76:
                    break;
                case 0x77:
                    break;
                case 0x78:
                    break;
                case 0x79:
                    break;
                case 0x7A:
                    break;
                case 0x7B:
                    break;
                case 0x7C:
                    break;
                case 0x7D:
                    break;
                case 0x7E:
                    break;
                case 0x7F:
                    break;
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                    break;
                case 0x88:
                case 0x89:
                case 0x8A:
                case 0x8B:
                case 0x8C:
                case 0x8D:
                case 0x8E:
                case 0x8F:
                    break;
                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                    break;
                case 0x98:
                case 0x99:
                case 0x9A:
                case 0x9B:
                case 0x9C:
                case 0x9D:
                case 0x9E:
                case 0x9F:
                    break;
                case 0xA0:
                case 0xA1:
                case 0xA2:
                case 0xA3:
                case 0xA4:
                case 0xA5:
                case 0xA6:
                case 0xA7:
                    break;
                case 0xA8:
                case 0xA9:
                case 0xAA:
                case 0xAB:
                case 0xAC:
                case 0xAD:
                case 0xAE:
                case 0xAF:
                    break;
                case 0xB0:
                case 0xB1:
                case 0xB2:
                case 0xB3:
                case 0xB4:
                case 0xB5:
                case 0xB6:
                case 0xB7:
                    break;
                case 0xB8:
                case 0xB9:
                case 0xBA:
                case 0xBB:
                case 0xBC:
                case 0xBD:
                case 0xBE:
                case 0xBF:
                    break;
                case 0xC0:
                    break;
                case 0xC1:
                    break;
                case 0xC2:
                    break;
                case 0xC3:
                    break;
                case 0xC4:
                    break;
                case 0xC5:
                    break;
                case 0xC6:
                    break;
                case 0xC7:
                    break;
                case 0xC8:
                    break;
                case 0xC9:
                    break;
                case 0xCA:
                    break;
                case 0xCB:
                    break;
                case 0xCC:
                    break;
                case 0xCD:
                    break;
                case 0xCE:
                    break;
                case 0xCF:
                    break;
                case 0xD0:
                    break;
                case 0xD1:
                    break;
                case 0xD2:
                    break;
                case 0xD4:
                    break;
                case 0xD5:
                    break;
                case 0xD6:
                    break;
                case 0xD7:
                    break;
                case 0xD8:
                    break;
                case 0xD9:
                    break;
                case 0xDA:
                    break;
                case 0xDC:
                    break;
                case 0xDE:
                    break;
                case 0xDF:
                    break;
                case 0xE0:
                    break;
                case 0xE1:
                    break;
                case 0xE2:
                    break;
                case 0xE5:
                    break;
                case 0xE6:
                    break;
                case 0xE7:
                    break;
                case 0xE8:
                    break;
                case 0xE9:
                    break;
                case 0xEA:
                    break;
                case 0xEE:
                    break;
                case 0xEF:
                    break;
                case 0xF0:
                    break;
                case 0xF1:
                    break;
                case 0xF2:
                    break;
                case 0xF3:
                    break;
                case 0xF5:
                    break;
                case 0xF6:
                    break;
                case 0xF7:
                    break;
                case 0xF8:
                    break;
                case 0xF9:
                    break;
                case 0xFA:
                    break;
                case 0xFB:
                    break;
                case 0xFE:
                    break;
                case 0xFF:
                    break;
            }

            void Xor(byte opcode) {

            }

            void And(byte opcode) {

            }

            void Sub(byte opcode) {

            }

            void Add(byte opcode) {

            }

            void Or(byte opcode) {

            }

            void Adc(byte opcode) {

            }

            void Sbc(byte opcode) {

            }

            void Cp(byte opcode) {

            }

            void Ld(byte opcode) {

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
        }
	}
}
