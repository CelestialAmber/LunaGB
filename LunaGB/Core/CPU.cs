using System;
namespace LunaGB.Core
{
	public class CPU {

		public byte A, B, C, D, E, F, H, L; //8-bit registers
        public byte sp, pc; //stack pointer, program counter
        public bool flagZ, flagNZ, flagC, flagNC; //condition flags
        public int cycles;

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
            byte opcode = memory.GetByte(pc);

            switch (opcode) {
                case 0x00:
                    break;
                case 0x01:
                    break;
                case 0x02:
                    break;
                case 0x03:
                    break;
                case 0x04:
                    break;
                case 0x05:
                    break;
                case 0x06:
                    break;
                case 0x07:
                    break;
                case 0x08:
                    break;
                case 0x09:
                    break;
                case 0x0A:
                    break;
                case 0x0B:
                    break;
                case 0x0C:
                    break;
                case 0x0D:
                    break;
                case 0x0E:
                    break;
                case 0x0F:
                    break;
                case 0x10:
                    break;
                case 0x11:
                    break;
                case 0x12:
                    break;
                case 0x13:
                    break;
                case 0x14:
                    break;
                case 0x15:
                    break;
                case 0x16:
                    break;
                case 0x17:
                    break;
                case 0x18:
                    break;
                case 0x19:
                    break;
                case 0x1A:
                    break;
                case 0x1B:
                    break;
                case 0x1C:
                    break;
                case 0x1D:
                    break;
                case 0x1E:
                    break;
                case 0x1F:
                    break;
                case 0x20:
                    break;
                case 0x21:
                    break;
                case 0x22:
                    break;
                case 0x23:
                    break;
                case 0x24:
                    break;
                case 0x25:
                    break;
                case 0x26:
                    break;
                case 0x27:
                    break;
                case 0x28:
                    break;
                case 0x29:
                    break;
                case 0x2A:
                    break;
                case 0x2B:
                    break;
                case 0x2C:
                    break;
                case 0x2D:
                    break;
                case 0x2E:
                    break;
                case 0x2F:
                    break;
                case 0x30:
                    break;
                case 0x31:
                    break;
                case 0x32:
                    break;
                case 0x33:
                    break;
                case 0x34:
                    break;
                case 0x35:
                    break;
                case 0x36:
                    break;
                case 0x37:
                    break;
                case 0x38:
                    break;
                case 0x39:
                    break;
                case 0x3A:
                    break;
                case 0x3B:
                    break;
                case 0x3C:
                    break;
                case 0x3D:
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
        }
	}
}
