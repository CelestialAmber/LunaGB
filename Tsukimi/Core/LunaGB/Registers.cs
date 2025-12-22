using System;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography;

namespace Tsukimi.Core.LunaGB {

	//TODO: Unused bits generally seem to be 1. Is this always true?
	public class Registers {
		//Bits 6-7 are unused, bits 0-3 are read only
		public byte P1 {
			get {
				byte reg = (byte)(P10 | (P11 << 1) | (P12 << 2) | (P13 << 3));
				reg |= (byte)((P14 << 4) | (P15 << 5) | (1 << 6) | (1 << 7));
				return reg;
			 }
			set {
				P14 = (value >> 4) & 1;
				P15 = (value >> 5) & 1;
			}
		}

		//P1 register bits
		public int P10; //Right/A, bit 0 (read only)
		public int P11; //Left/B, bit 1 (read only)
		public int P12; //Up/select, bit 2 (read only)
		public int P13; //Down/start, bit 3 (read only)
		public int P14; //Dpad select, bit 4
		public int P15; //Button select, bit 5


		public byte SB; //0xFF01
		//0xFF02
		public byte SC{
			get{
				byte reg = (byte)(SIO_CLK | (SIO_FAST << 1) | (SIO_EN << 7) | 0b01111100);
				return reg;
			}
			set{
				SIO_CLK = value & 1;
				SIO_FAST = (value >> 1) & 1;
				SIO_EN = (value >> 7) & 1;
			}
		}

		//SC flags
		public int SIO_CLK;
		public int SIO_FAST;
		public int SIO_EN;


		public byte DIV; //0xFF04
		public byte TIMA; //0xFF05
		public byte TMA; //0xFF06

		//0xFF07
		public byte TAC{
			get{
				byte reg = (byte)(TAC_CLK | (TAC_EN << 2) | 0b11111000);
				return reg;
			}
			set{
				TAC_CLK = value & 3;
				TAC_EN = (value >> 2) & 1;
			}
		}

		//TAC flags
		public int TAC_CLK;
		public int TAC_EN;

		//0xFF0F
		public byte IF{
			get{
				byte reg = (byte)(IF_VBLANK | (IF_STAT << 1) | (IF_TIMER << 2) | (IF_SERIAL << 3) | (IF_JOYPAD << 4));
				reg |= 0b11100000;
				return reg;
			}
			set{
				IF_VBLANK = value & 1;
				IF_STAT = (value >> 1) & 1;
				IF_TIMER = (value >> 2) & 1;
				IF_SERIAL = (value >> 3) & 1;
				IF_JOYPAD = (value >> 4) & 1;
			}
		}

		//IF flags
		public int IF_VBLANK;
		public int IF_STAT;
		public int IF_TIMER;
		public int IF_SERIAL;
		public int IF_JOYPAD;

		//APU registers
		
		//Channel 1 registers

		//0xFF10 - Channel 1 sweep
		//bit 7 is unused
		public byte NR10{
			get{
				byte reg = (byte)(ch1SweepIndividualStep | (ch1SweepDir << 3) | (ch1SweepPace << 4));
				reg |= 0b10000000;
				return reg;
			}
			set{
				ch1SweepIndividualStep = value & 7;
				ch1SweepDir = (value >> 3) & 1;
				ch1SweepPace = (value >> 4) & 7;
			}
		}
		//0xFF11 - Channel 1 length timer/duty cycle
		public byte NR11{
			get{
				byte reg = (byte)(channelWaveDuty[0] << 6);
				reg |= 0b00111111;
				return reg;
			}
			set{
				channelInitialLengthTimer[0] = value & 0b111111;
				channelWaveDuty[0] = (value >> 6) & 3;
			}
		}
		//0xFF12 - Channel 1 volume/envelope
		public byte NR12{
			get{
				byte reg = (byte)(channelEnvSweepPace[0] | (channelEnvDir[0] << 3) | (channelInitialVolume[0] << 4));
				return reg;
			}
			set{
				channelEnvSweepPace[0] = value & 0b111;
				channelEnvDir[0] = (value >> 3) & 1;
				channelInitialVolume[0] = (value >> 4) & 0b1111;
			}
		}
		//0xFF13 - Channel 1 period low
		public byte NR13{
			get{
				//write only, return 0xFF?
				return 0xFF;
			}
			set{
				channelPeriod[0] = (channelPeriod[0] & ~0xFF) | value;
			}
		}
		//0xFF14 - Channel 1 period high/control
		//bits 3-5 are unused
		public byte NR14{
			get{
				byte reg = (byte)(channelLengthEnable[0] << 6);
				reg |= 0b10111111;
				return reg;
			}
			set{
				channelPeriod[0] = (channelPeriod[0] & 0xFF) | ((value & 0b111) << 8);
				channelLengthEnable[0] = (value >> 6) & 1;
				channelTrigger[0] = (value >> 7) & 1;
			}
		}

		//Channel 2 registers

		//0xFF21 - Channel 2 length timer/duty cycle
		public byte NR21{
			get{
				byte reg = (byte)(channelWaveDuty[1] << 6);
				reg |= 0b00111111;
				return reg;
			}
			set{
				channelInitialLengthTimer[1] = value & 0b111111;
				channelWaveDuty[1] = (value >> 6) & 3;
			}
		}
		//0xFF22 - Channel 2 volume/envelope
		public byte NR22{
			get{
				byte reg = (byte)(channelEnvSweepPace[1] | (channelEnvDir[1] << 3) | (channelInitialVolume[1] << 4));
				return reg;
			}
			set{
				channelEnvSweepPace[1] = value & 0b111;
				channelEnvDir[1] = (value >> 3) & 1;
				channelInitialVolume[1] = (value >> 4) & 0b1111;
			}
		}
		//0xFF23 - Channel 2 period low
		public byte NR23{
			get{
				//write only, return 0xFF?
				return 0xFF;
			}
			set{
				channelPeriod[1] = (channelPeriod[1] & ~0xFF) | value;
			}
		}
		//0xFF24 - Channel 2 period high/control
		//bits 3-5 are unused
		public byte NR24{
			get{
				byte reg = (byte)(channelLengthEnable[1] << 6);
				reg |= 0b10111111;
				return reg;
			}
			set{
				channelPeriod[1] = (channelPeriod[1] & 0xFF) | ((value & 0b111) << 8);
				channelLengthEnable[1] = (value >> 6) & 1;
				channelTrigger[1] = (value >> 7) & 1;
			}
		}

		//Channel 3 registers

		//0xFF1A - Channel 3 DAC enable
		//bits 0-6 are unused
		public byte NR30{
			get{
				byte reg = (byte)(ch3DACOnOff << 7);
				reg |= 0b01111111;
				return reg;
			}
			set{
				ch3DACOnOff = (value >> 7) & 1;
			}
		}
		//0xFF1B - Channel 3 length timer
		public byte NR31{
			get{
				//write only, return 0xFF?
				return 0xFF;
			}
			set{
				channelInitialLengthTimer[2] = value;
			}
		}
		//0xFF1C - Channel 3 output level
		//bits 0-4,7 are unused
		public byte NR32{
			get{
				byte reg = (byte)(ch3OutputLevel << 5);
				reg |= 0b10011111;
				return reg;
			}
			set{
				ch3OutputLevel = (value >> 5) & 0b11;
			}
		}
		//0xFF1D - Channel 3 period low
		public byte NR33{
			get{
				//write only, return 0xFF?
				return 0xFF;
			}
			set{
				channelPeriod[2] = (channelPeriod[2] & ~0xFF) | value;
			}
		}
		//0xFF1E - Channel 3 period high/control
		public byte NR34{
			get{
				byte reg = (byte)(channelLengthEnable[2] << 6);
				reg |= 0b10111111;
				return reg;
			}
			set{
				channelPeriod[2] = (channelPeriod[2] & 0xFF) | ((value & 0b111) << 8);
				channelLengthEnable[2] = (value >> 6) & 1;
				channelTrigger[2] = (value >> 7) & 1;
			}
		}

		//Channel 4 registers

		//0xFF20 - Channel 4 length timer
		//bits 6-7 are unused
		public byte NR41{
			get{
				//write only, return 0xFF?
				return 0xFF;
			}
			set{
				channelInitialLengthTimer[3] = value & 0b111111;
			}
		}
		//0xFF21 - Channel 4 volume/envelope
		public byte NR42{
			get{
				byte reg = (byte)(channelEnvSweepPace[3] | (channelEnvDir[3] << 3) | (channelInitialVolume[3] << 4));
				return reg;
			}
			set{
				channelEnvSweepPace[3] = value & 0b111;
				channelEnvDir[3] = (value >> 3) & 1;
				channelInitialVolume[3] = (value >> 4) & 0b1111;
			}
		}
		//0xFF22 - Channel 4 frequency/randomness
		public byte NR43{
			get{
				byte reg = (byte)(ch4ClockDivider | (ch4LFSRWidth << 3) | (ch4ClockShift << 4));
				return reg;
			}
			set{
				ch4ClockDivider = value & 0b111;
				ch4LFSRWidth = (value >> 3) & 1;
				ch4ClockDivider = (value >> 4) & 0b1111;
			}
		}
		//0xFF23 - Channel 4 control
		//bits 0-5 are unused
		public byte NR44{
			get{
				byte reg = (byte)(channelLengthEnable[3] << 6);
				reg |= 0b10111111;
				return reg;
			}
			set{
				channelLengthEnable[3] = (value >> 6) & 1;
				channelTrigger[3] = (value >> 7) & 1;
			}
		}


		//NRx1 flags

		//the channel 3/4 entries are unused
		public int[] channelWaveDuty = new int[4]; //bits 6-7 (channels 1/2)
		//write only
		public int[] channelInitialLengthTimer = new int[4]; //bits 0-5 for channels 1/2/4, 0-7 for channel 3

		//NRx2 flags
		
		//the channel 3 entry for the following is unused
		public int[] channelEnvSweepPace = new int[4]; //bits 0-2
		public int[] channelEnvDir = new int[4]; //bit 3
		public int[] channelInitialVolume = new int[4]; //bits 4-7
		
		//NRx4 flags

		//only the first 3 entries are used
		public int[] channelPeriod = new int[4]; //11 bit values made up of NRx3 for lower 8 bits, and bits 0-2 in NRx4 for upper 3 bits
		public int[] channelLengthEnable = new int[4]; //bit 6
		public int[] channelTrigger = new int[4]; //bit 7

		//NR10 flags
		public int ch1SweepIndividualStep; //bits 0-2
		public int ch1SweepDir; //bit 3
		public int ch1SweepPace; //bits 4-6

		//NR30 flags
		public int ch3DACOnOff; //bit 7
		//NR32 flags
		public int ch3OutputLevel; //bits 5-6

		//NR43 flags
		public int ch4ClockDivider; //bits 0-2
		public int ch4LFSRWidth; //bit 3
		public int ch4ClockShift; //bits 4-7

		//Global control registers

		//0xFF24 - Master volume/VIN panning
		public byte NR50{
			get{
				byte reg = (byte)(rightVol | (vinRight << 3) | (leftVol << 4) | (vinLeft << 7));
				return reg;
			}
			set{
				rightVol = value & 0b111;
				vinRight = (value >> 3) & 1;
				leftVol = (value >> 4) & 0b111;
				vinLeft = (value >> 7) & 1;
			}
		}

		//NR50 flags
		public int rightVol; //bits 0-2
		public int vinRight; //bit 3
		public int leftVol; //bits 4-6
		public int vinLeft; //bit 7

		//0xFF25 - Sound panning
		public byte NR51{
			get{
				byte reg = (byte)(channelRightPan[0] | (channelRightPan[1] << 1) | (channelRightPan[2] << 2) | (channelRightPan[3] << 3));
				reg |= (byte)((channelLeftPan[0] << 4) | (channelLeftPan[1] << 5) | (channelLeftPan[2] << 6) | (channelLeftPan[3] << 7));
				return reg;
			}
			set{
				channelRightPan[0] = value & 1;
				channelRightPan[1] = (value >> 1) & 1;
				channelRightPan[2] = (value >> 2) & 1;
				channelRightPan[3] = (value >> 3) & 1;
				channelLeftPan[0] = (value >> 4) & 1;
				channelLeftPan[1] = (value >> 5) & 1;
				channelLeftPan[2] = (value >> 6) & 1;
				channelLeftPan[3] = (value >> 7) & 1;
			}
		}

		//NR51 flags
		public int[] channelRightPan = new int[4]; //bits 0-3
		public int[] channelLeftPan = new int[4]; //bits 4-7

		//0xFF26 - Audio master control
		//bits 4-6 are unused
		public byte NR52{
			get{
				byte reg = (byte)(channelEnabled[0] | (channelEnabled[1] << 1) | (channelEnabled[2] << 2) | (channelEnabled[3] << 3));
				reg |= (byte)((audioOnOff << 7) | 0b01110000);
				return reg;
			}
			set{
				audioOnOff = (value >> 7) & 1;
			}
		}

		//NR52 flags
		public int[] channelEnabled = new int[4]; //bits 0-3, read only
		public int audioOnOff; //bit 4

		//0xFF30-FF40
		public byte[] waveRam = new byte[16];

		//PPU registers

		//lcdc control (0xFF40)
		public byte LCDC{
			get{
				byte reg = (byte)(bgWindowEnablePriority | (objEnable << 1) | (objSize << 2) | (bgTilemapArea << 3));
				reg |= (byte)((bgWindowTileDataArea << 4) | (windowEnable << 5) | (windowTilemapArea << 6) | (lcdcEnable << 7));
				return reg;
			}
			set{
				bgWindowEnablePriority = value & 1;
				objEnable = (value >> 1) & 1;
				objSize = (value >> 2) & 1;
				bgTilemapArea = (value >> 3) & 1;
				bgWindowTileDataArea = (value >> 4) & 1;
				windowEnable = (value >> 5) & 1;
				windowTilemapArea = (value >> 6) & 1;
				lcdcEnable = (value >> 7) & 1;

				//If lcd enable is set to 0, the lcd mode bits in STAT are reset
				if(lcdcEnable == 0){
					LCD_MODE = 0;
				}
			}
		}

		//LCDC flags
		public int bgWindowEnablePriority; //BG/Window enable/priority (bit 0)
		public int objEnable; //OBJ enable (bit 1)
		public int objSize; //OBJ size (bit 2)
		public int bgTilemapArea; //BG Tilemap Area (bit 3)
		public int bgWindowTileDataArea; //BG/Window tile data area (bit 4)
		public int windowEnable; //Window enable (bit 5)
		public int windowTilemapArea; //Window tilemap area (bit 6)
		public int lcdcEnable; //LCD enable (bit 7)

		//PPU status (0xFF41)
		//Bit 7 is unused, bits 0-2 are read only
		public byte STAT{
			get{
				byte reg = (byte)(LCD_MODE | (LYC_STAT << 2) | (INTR_M0 << 3) | (INTR_M1 << 4));
				reg |= (byte)((INTR_M2 << 5) | (INTR_LYC << 6) | (1 << 7));
				return reg;
			}
			set{
				INTR_M0 = (value >> 3) & 1;
				INTR_M1 = (value >> 4) & 1;
				INTR_M2 = (value >> 5) & 1;
				INTR_LYC = (value >> 6) & 1;
			}
		}

		//STAT flags
		public int LCD_MODE; //bits 0-1 (read only)
		public int LYC_STAT; //bit 2 (read only)
		public int INTR_M0; //bit 3
		public int INTR_M1; //bit 4
		public int INTR_M2; //bit 5
		public int INTR_LYC; //bit 6



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


		//GBC registers

		//0xFF4D: prepare speed switch
		public byte KEY1{
			get{
				byte reg = (byte)(switchArmed | (currentSpeed << 7) | 0b01111110);
				return reg;
			}
			set{
				switchArmed = value & 1;
			}
		}

		//KEY1 flags
		public int switchArmed; //bit 0
		public int currentSpeed; //bit 7

		//0xFF4F: VRAM Bank
		public byte VBK{
			get{
				return (byte)(vramBank | 0b11111110);
			}
			set{
				vramBank = value & 1;
			}
		}

		public int vramBank;

		//VRAM DMA registers
		//0xFF51: lower 4 bits of vram dma source address
		//bits 0-3 are unused
		public byte HDMA1{
			get{
				//the lower 4 bits just return 0, the upper 4 return 1?
				return 0xF0;
			}
			set{
				vramDMASource = (vramDMASource & 0xFF0) | (value & 0xF);
			}
		}
		//0xFF52: upper 8 bits of vram dma source address
		public byte HDMA2{
			get{
				return 0xFF;
			}
			set{
				vramDMASource = (vramDMASource & 0xF) | (value << 8);
			}
		}
		//0xFF53: lower 4 bits of vram dma destination address
		//bits 0-3 are unused
		public byte HDMA3{
			get{
				//the lower 4 bits just return 0, the upper 4 return 1?
				return 0xF0;
			}
			set{
				vramDMADest = (vramDMADest & 0xFF0) | (value & 0xF);
			}
		}
		//0xFF54: upper 8 bits of vram dma destination address
		public byte HDMA4{
			get{
				return 0xFF;
			}
			set{
				vramDMADest = (vramDMADest & 0xF) | (value << 8);
			}
		}
		//0xFF55: VRAM DMA length/mode/start
		public byte HDMA5{
			get{
				byte reg = (byte)(vramDMALength | 0b10000000);
				return reg;
			}
			set{
				vramDMALength = value & 0b01111111;
				vramDMAMode = (value >> 7) & 1;
			}
		}

		public int vramDMASource;
		public int vramDMADest;
		public int vramDMAMode;
		public int vramDMALength;
		public bool vramDMAActive;

		//0xFF56: IR port
		public byte RP{
			get{
				byte reg = (byte)(irEmitting | (irRecieving << 1) | (irReadEnable << 6) | 0b00111100);
				return reg;
			}
			set{
				irEmitting = value & 1;
				irReadEnable = (value >> 6) & 0b11;
			}
		}

		public int irEmitting; //bit 0
		public int irRecieving; //bit 1
		public int irReadEnable; //bits 6-7

		//0xFF68: bg color palette specification
		//bit 6 is unused
		public byte BCPS{
			get{
				byte reg = (byte)(bgColorPaletteAddr | (bgColorPaletteAddrAutoIncrement << 7) | 0b10111111);
				return reg;
			}
			set{
				bgColorPaletteAddr = value & 0b111111;
				bgColorPaletteAddrAutoIncrement = (value >> 7) & 1;
			}
		}

		public int bgColorPaletteAddr; //bits 0-5
		public int bgColorPaletteAddrAutoIncrement; //bit 7

		//0xFF69: bg color palette data
		public byte BCPD{
			get{
				return 0;
			}
			set{
				bgColorPaletteMem[bgColorPaletteAddr] = value;
			}
		}

		//0xFF6A: object color palette specification
		public byte OCPS{
			get{
				byte reg = (byte)(objColorPaletteAddr | (objColorPaletteAddrAutoIncrement << 7) | 0b10111111);
				return reg;
			}
			set{
				objColorPaletteAddr = value & 0b111111;
				objColorPaletteAddrAutoIncrement = (value >> 7) & 1;
			}
		}

		public int objColorPaletteAddr; //bits 0-5
		public int objColorPaletteAddrAutoIncrement; //bit 7

		//0xFF6B: object color palette data
		public byte OCPD{
			get{
				return 0;
			}
			set{
				objColorPaletteMem[objColorPaletteAddr] = value;
			}
		}


		//bg/obj color palette memory
		public byte[] bgColorPaletteMem = new byte[64];
		public byte[] objColorPaletteMem = new byte[64];

		//0xFF6C: Object priority mode
		public byte OPRI{
			get{
				byte reg = (byte)(priorityMode | 0b11111110);
				return reg;
			}
			set{
				priorityMode = value & 1;
			}
		}

		public int priorityMode; //bit 0

		//0xFF70: WRAM Bank
		public byte SVBK{
			get{
				byte reg = (byte)(wramBank | 0b11111000);
				return reg;
			}
			set{
				wramBank = value & 0b111;
			}
		}

		public int wramBank; //bits 0-2

		//0xFF76: Digital outputs 1/2
		public byte PCM12{
			get{
				return (byte)(digitalOutputs[0] | (digitalOutputs[1] << 4));
			}
		}
		//0xFF77: Digital outputs 3/4
		public byte PCM34{
			get{
				return (byte)(digitalOutputs[2] | (digitalOutputs[3] << 4));
			}
		}

		public byte[] digitalOutputs = new byte[4];

		//0xFFFF
		public byte IE{
			get{
				byte reg = (byte)(IE_VBLANK | (IE_STAT << 1) | (IE_TIMER << 2) | (IE_SERIAL << 3) | (IE_JOYPAD << 4));
				return reg;
			}
			set{
				IE_VBLANK = value & 1;
				IE_STAT = (value >> 1) & 1;
				IE_TIMER = (value >> 2) & 1;
				IE_SERIAL = (value >> 3) & 1;
				IE_JOYPAD = (value >> 4) & 1;
			}
		}

		//IE flags
		public int IE_VBLANK;
		public int IE_STAT;
		public int IE_TIMER;
		public int IE_SERIAL;
		public int IE_JOYPAD;

		public void Init(bool onGBC){
			//Init I/O registers
			P1 = 0xCF; //0xC7/CF on SGB/GBC
			SB = 0x00;
			SC = (byte)(onGBC ? 0x7F : 0x7E); //0x7F on GBC, 0x7E otherwise
			DIV = 0xAB; //0x18 on DMG0, nondeterministic on SGB/GBC
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
			NR52 = 0xF1; //F0 on sgb
			for(int i = 0; i < 16; i++){
				waveRam[i] = 0;
			}
			//PPU registers
			LCDC = 0x91;
			STAT = 0x85; //nondeterministic on sgb/gbc, 0x81 on dmg0
			SCY = 0x00;
			SCX = 0x00;
			LY = 0x00; //nondeterministic on sgb/gbc, 0x91 on dmg0
			LYC = 0x00;
			DMA = (byte)(onGBC ? 0x00 : 0xFF); //0x00 on gbc
			BGP = 0xFC;
			//TODO: uninitialized, set for now to 0
			OBP0 = 0x00;
			OBP1 = 0x00;
			WY = 0x00;
			WX = 0x00;
			
			//GBC registers
			if(onGBC){
				KEY1 = 0x7E;
				vramBank = 0; //VBK = 0xFE
				HDMA1 = 0xFF;
				HDMA2 = 0xFF;
				HDMA3 = 0xFF;
				HDMA4 = 0xFF;
				HDMA5 = 0xFF;
				RP = 0x00;
				BCPS = 0x00;
				OCPS = 0x00;

				//Initialize GBC color palette memory to all white
				for(int i = 0; i < 64; i++){
					bgColorPaletteMem[i] = 0b01111111;
					objColorPaletteMem[i] = 0b01111111;
				}

				wramBank = 0; //SVBK = 0xF8

				//PCM12/PCM34
				for(int i = 0; i < 4; i++){
					digitalOutputs[i] = 0;
				}
			}

			IE = 0x00;
		}
	}
}