using System;
namespace LunaGB.Core
{
	public enum IORegister {
		//I/O register address constants
		P1 = 0x00,
		SB = 0x01,
		SC = 0x02,
		DIV = 0x04,
		TIMA = 0x05,
		TMA = 0x06,
		TAC = 0x07,
		IF = 0x0F,

		//maybe also include these here?
		//0x10-26: NR registers
		//0x30-3F: WAV00-15

		//bit 0: BG_EN, 1: OBJ_EN, 2: OBJ_SIZE, 3: BG_MAP, 4: TILE_SEL, 5: WIN_EN, 6: WIN_MAP, 7: LCD_EN
		LCDC = 0x40,
		STAT = 0x41,
		SCY = 0x42,
		SCX = 0x43,
		LY = 0x44,
		LYC = 0x45,
		DMA = 0x46,
		BGP = 0x47,
		OBP0 = 0x48,
		OBP1 = 0x49,
		WY = 0x4A,
		WX = 0x4B,
		KEY1 = 0x4D,
		VBK = 0x4F,
		HDMA1 = 0x51,
		HDMA2 = 0x52,
		HDMA3 = 0x53,
		HDMA4 = 0x54,
		HDMA5 = 0x55,
		RP = 0x56,
		BCPS = 0x68,
		BCPD = 0x69,
		OCPS = 0x6A,
		OCPD = 0x6B
		//GBC registers
		//SVBK = 0x70,
		//PCM12 = 0x76,
		//PCM34 = 0x77
	}
}

