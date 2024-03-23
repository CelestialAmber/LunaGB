using System;
using System.Security.Cryptography;

namespace LunaGB.Core {

	public class Registers {
		public byte _P1; //0xFF00
		//Only bits 4/5 are writeable
		public byte P1 {
			get { return _P1; }
			set { RegWriteMask(ref _P1, value, 0b00110000); }
		}
		public byte SB; //0xFF01
		public byte SC; //0xFF02
		public byte DIV; //0xFF04
		public byte TIMA; //0xFF05
		public byte TMA; //0xFF06
		public byte TAC; //0xFF07
		public byte IF; //0xFF0F

		//APU registers
		public byte NR10;
		public byte NR11;
		public byte NR12;
		public byte NR13;
		public byte NR14;
		public byte NR21;
		public byte NR22;
		public byte NR23;
		public byte NR24;
		public byte NR30;
		public byte NR31;
		public byte NR32;
		public byte NR33;
		public byte NR34;
		public byte NR41;
		public byte NR42;
		public byte NR43;
		public byte NR44;
		public byte NR50;
		public byte NR51;
		public byte NR52;
		public byte[] waveRam = new byte[16];

		//PPU registers
		public byte LCDC; //lcdc control (0xFF40)

		//LCDC flags
		//BG/Window enable/priority (bit 0)
		public int bgWindowEnablePriority{
			get{ return (LCDC >> 0) & 1; }
		}

		//OBJ enable (bit 1)
		public int objEnable {
			get{ return (LCDC >> 1) & 1; }
		}

		//OBJ size (bit 2)
		public int objSize {
			get{ return (LCDC >> 2) & 1; }
		}

		//BG Tilemap Area (bit 3)
		public int bgTilemapArea {
			get{ return (LCDC >> 3) & 1; }
		}

		//BG/Window tile data area (bit 4)
		public int bgWindowTileDataArea {
			get{ return (LCDC >> 4) & 1; }
		}

		//Window enable (bit 5)
		public int windowEnable {
			get{ return (LCDC >> 5) & 1; }
		}

		//Window tilemap area (bit 6)
		public int windowTilemapArea {
			get{ return (LCDC >> 6) & 1; }
		}

		//LCD enable (bit 7)
		public int lcdcEnable {
			get{ return (LCDC >> 7) & 1; }
		}

		public byte STAT; //ppu status (0xFF41)
		public byte SCY; //scroll y (0xFF42)
		public byte SCX; //scroll x (0xFF43)
		public byte LY; //line y (0xFF44)
		public byte LYC; //line y compare (0xFF45)
		public byte DMA; //oam dma (0xFF46)
		public byte BGP; //bg palette (0xFF47)
		public byte OBP0; //obj palette 0 (0xFF48)
		public byte OBP1; //obj palette 1 (0xFF49)
		public byte WY; //window y (0xFF4A)
		public byte WX; //window x (0xFF4B)

		public byte KEY1; //0xFF4D
		public byte VBK; //0xFF4F
		public byte HDMA1; //0xFF51
		public byte HDMA2; //0xFF52
		public byte HDMA3; //0xFF53
		public byte HDMA4; //0xFF54
		public byte HDMA5; //0xFF55
		public byte RP; //0xFF56
		public byte BCPS; //0xFF68
		public byte BCPD; //0xFF69
		public byte OCPS; //0xFF6A
		public byte OCPD; //0xFF6B
		public byte SVBK; //0xFF70
		public byte PCM12; //0xFF76
		public byte PCM34; //0xFF77

		public byte IE; //0xFFFF

		public void Init(){
			//Init I/O registers (DMG values)
			P1 = 0xCF;
			SB = 0x00;
			SC = 0x7E;
			DIV = 0xAB;
			TIMA = 0x00;
			TMA = 0x00;
			TAC = 0x00;
			IF = 0xE1;
			//APU registers (same on all systems)
			NR10 = 0x80;
			NR11 = 0xBF;
			NR12 = 0xF3;
			NR13 = 0xFF;
			NR14 = 0xBF;
			NR21 = 0x3F;
			NR22 = 0x00;
			NR23 = 0xFF;
			NR24 = 0xBF;
			NR30 = 0x7F;
			NR31 = 0xFF;
			NR32 = 0x9F;
			NR33 = 0xFF;
			NR34 = 0xBF;
			NR41 = 0xFF;
			NR42 = 0x00;
			NR43 = 0x00;
			NR44 = 0xBF;
			NR50 = 0x77;
			NR51 = 0xF3;
			NR52 = 0xF1; //F0 on sgb/sgb2
			//PPU registers
			LCDC = 0x91;
			STAT = 0x85;
			SCY = 0x00;
			SCX = 0x00;
			LY = 0x00;
			LYC = 0x00;
			DMA = 0xFF;
			BGP = 0xFC;
			//TODO: uninitialized, set for now to 0
			OBP0 = 0x00;
			OBP1 = 0x00;
			WY = 0x00;
			WX = 0x00;
			//GBC registers
			for(int i = 0; i < 16; i++){
				waveRam[i] = 0;
			}
			KEY1 = 0x7E;
			VBK = 0xFE;
			HDMA1 = 0xFF;
			HDMA2 = 0xFF;
			HDMA3 = 0xFF;
			HDMA4 = 0xFF;
			HDMA5 = 0xFF;
			RP = 0x00;
			BCPS = 0x00;
			BCPD = 0x00;
			OCPS = 0x00;
			OCPD = 0x00;
			SVBK = 0xF8;
			PCM12 = 0x00;
			PCM34 = 0x00;
			IE = 0x00;
		}

		//Updates the bits specified by the mask in the given register value from the given value.
		//For example, P1 is only writeable to bits 4/5, so its mask is 0b00110000, only allowing those
		//bits to be updated.
		void RegWriteMask(ref byte reg, byte val, byte mask){
			reg = (byte)((reg & ~mask) | (val & mask));
		}

		public int GetBit(byte reg, int bit){
			return (reg >> bit) & 1;
		}

		public void SetBit(ref byte reg, int bit, int val){
			reg = (byte)((reg & ~(1 << bit)) | (val << bit));
		}
		

	}


}