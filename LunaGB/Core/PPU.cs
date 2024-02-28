using System;
using System.Data.Common;
using LunaGB.Graphics;

namespace LunaGB.Core
{
	public class PPU
	{
		public enum PPUMode{
			HBlank = 0,
			VBlank = 1,
			OAM = 2,
			Drawing = 3
		}


		Memory memory;
		public LunaImage display;

		byte ly = 0;
		PPUMode mode;
		public const int cyclesPerScanline = 456; //each scanline takes 456 cycles
		public int scanlineCycleCount = 0;
		bool blockStatIrqs = false;
		bool switchedMode = false; //used for checking the stat conditions, should change later

		public PPU(Memory memory)
		{
			display = new LunaImage(160,144);
			this.memory = memory;
			
		}

		public void Init(){
			ly = 0;
			scanlineCycleCount = 0;
			mode = PPUMode.OAM; //not sure if this is right but whatever
			blockStatIrqs = false;
		}

		//TODO:
		//-The cpu should really eventually be rewritten to just be cycle accurate
		//so the PPU can step cycle by cycle instead.
		//-Maybe move the PPU to its own thread?
		//-The different PPU actions should also actually be done during the respective modes instead
		//of faking it.
		public void Step(int cpuCyclesPassed)
		{
			//TODO: properly emulate PPU behavior lol
			/*
			switch(mode){
				case PPUMode.HBlank:
				break;
				...
			}
			*/

			byte stat = memory.GetIOReg(IORegister.STAT);
			int mode0IntSelect = (stat >> 3) & 1;
			int mode1IntSelect = (stat >> 4) & 1;
			int mode2IntSelect = (stat >> 5) & 1;
			int lycIntSelect = (stat >> 6) & 1;

			//Increment the current scanline cycle count by how many cycles the last instruction took
			scanlineCycleCount += cpuCyclesPassed;

			//If we're not in vblank, check whether we should change mode
			if(mode != PPUMode.VBlank){
				//If 80 or more cycles have passed and we're in mode 2 (oam), change to mode 3 (drawing)
				if(scanlineCycleCount >= 80 && mode == PPUMode.OAM){
					SetMode(PPUMode.Drawing);
				}

				//If we're done drawing the current scanline, change to mode 0 (hblank)
				//Drawing takes between 172-289 cycles, so for now let's assume it took the minimum.
				if(scanlineCycleCount >= 252 && mode == PPUMode.Drawing){
					SetMode(PPUMode.HBlank);
					//If the mode 0 condition was enabled and we changed to mode 0, request a stat interrupt
					if(mode0IntSelect == 1) RequestSTATInterrupt();
				}
			}
			
			//If 456 or more cycles have passed, go to the next scanline
			if(scanlineCycleCount > cyclesPerScanline){
				scanlineCycleCount %= cyclesPerScanline;
				
				ly++;

				SetMode(PPUMode.OAM);

				//If ly is 144, we're at the start of vblank
				if(ly == 144){
					SetMode(PPUMode.VBlank);
					//Request a VBlank interrupt
					RequestVBlankInterrupt();
					//If the mode 1 condition was enabled and we changed to mode 1, request a stat interrupt
					if(mode1IntSelect == 1) RequestSTATInterrupt();
				}
				//If ly is 154, we're at the end of vblank, reset ly to 0
				if(ly == 154){
					ly = 0;
				}
				memory.SetIOReg(IORegister.LY, ly);

				//If the mode 2 condition is enabled and we changed to mode 2, request a stat interrupt
				if(mode == PPUMode.OAM && mode2IntSelect == 1){
					RequestSTATInterrupt();
				}

			}

			int lyc = memory.GetIOReg(IORegister.LYC);
			int lycFlag = ly == lyc ? 1 : 0;

			memory.SetHRAMBit(0xFF00 + (int)IORegister.STAT, 2, lycFlag);

			//If the lyc condition is enabled and ly == lyc, request a stat interrupt
			if(lycFlag == 1 && lycIntSelect == 1){
				RequestSTATInterrupt();
			}

			//If none of the conditions for a stat irq are met, disable stat irq blocking
			if(!(mode0IntSelect == 1 && mode == PPUMode.HBlank) && !(mode1IntSelect == 1 && mode == PPUMode.VBlank) && !(mode2IntSelect == 1 && mode == PPUMode.OAM) && !(lycIntSelect == 1 && lycFlag == 1)){
				blockStatIrqs = false;
			}
		}

		void SetMode(PPUMode newMode){
			mode = newMode;
			int modeVal = (int)mode;
			//Update the mode value in STAT (bits 0-1)
			memory.SetHRAMBit(0xFF00 + (int)IORegister.STAT,0,modeVal & 1);
			memory.SetHRAMBit(0xFF00 + (int)IORegister.STAT,0,(modeVal >> 1) & 1);
		}

		void RequestVBlankInterrupt(){
			memory.SetHRAMBit(0xFF00 + (int)IORegister.IF, 0, 1);
		}

		void RequestSTATInterrupt(){
			if(!blockStatIrqs){
				blockStatIrqs = true;
				memory.SetHRAMBit(0xFF00 + (int)IORegister.IF, 1, 1);
			}
		}


		byte[] bgTilemap = new byte[0x400];
		byte[] tileData = new byte[0x1000];

		Color[] palette = {new Color(255,255,255), new Color(170,170,170), new Color(85,85,85), new Color(0,0,0)};

		//Renders the entire background at once. This assumes the scroll position is (0,0).
		//This is temporary and will be replaced later.
		public void DrawEntireBackground(){
			byte lcdc = memory.GetIOReg(IORegister.LCDC);
			int bgTileMapArea = (lcdc >> 3) & 1;
			int bgTileDataArea = (lcdc >> 4) & 1;
			int tileDataStartAddress = bgTileDataArea == 1 ? 0x8000 : 0x8800;

			//Store the tilemap/tile data in temp arrays
			for(int i = 0; i < 0x800; i++){
				tileData[i] = memory.GetByte(tileDataStartAddress + i);
			}

			int tilemapStartAddress = bgTileMapArea == 0 ? 0x9800 : 0x9C00;

			for(int i = 0; i < 0x400; i++){
				bgTilemap[i] = memory.GetByte(tilemapStartAddress + i);
			}

			int scrollX = memory.GetIOReg(IORegister.SCX);
			int scrollY = memory.GetIOReg(IORegister.SCY);

			for(int y = 0; y < 18; y++){
				for(int x = 0; x < 20; x++){
					int tileX = x;
					int tileY = y;
					int tilemapByteIndex = tileY*32 + tileX;
					byte tileIndex = bgTilemap[tilemapByteIndex];
					DrawEntireTile(x, y, tileIndex);
				}
			}
		}

		//Draws an entire tile at once.
		void DrawEntireTile(int xPos, int yPos, int tileIndex){
			int tileDataOffset = tileIndex*16;

			for(int y = 0; y < 8; y++){
				byte loByte = tileData[tileDataOffset + y*2];
				byte hiByte = tileData[tileDataOffset + y*2 + 1];
				for(int x = 0; x < 8; x++){
					int lo = (loByte >> (7-x)) & 1;
					int hi = (hiByte >> (7-x)) & 1;
					int palIndex = lo + (hi << 1);
					Color col = palette[palIndex];
					display.SetPixel(xPos*8 + x,yPos*8 + y, col);
				}
			}
		}
	}
}

