using System;
using System.Data.Common;
using System.Collections.Generic;
using Tsukimi.Graphics;
using System.Threading;
using System.Linq;

namespace Tsukimi.Core.LunaGB
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
		Display display;

		public byte ly = 0;
		int windowLine = 0; //Internal line counter for the window
		bool WY_latch = false;
		PPUMode mode;
		public const int cyclesPerScanline = 456; //each scanline takes 456 cycles
		public int scanlineCycleCount = 0;
		bool blockStatIrqs = false;
		public bool onGBC; //whether we're on gbc or not
		public bool finishedFrame;
		public bool lcdReenabled; //used to keep track of if the lcd was reenabled and this frame should be discarded


		//Arrays for storing the GB register color palettes for easier use
		byte[] bgPalette = new byte[4];
		byte[] objPalette0 = new byte[4];
		byte[] objPalette1 = new byte[4];

		public PPU(Memory memory, Display display)
		{
			this.display = display;
			this.memory = memory;
			
		}

		public void Init(bool onGBC){
			this.onGBC = onGBC;
			ly = 0;
			WY_latch = false;
			finishedFrame = false;
			lcdReenabled = false;
			windowLine = 0;
			scanlineCycleCount = 0;
			SetMode(PPUMode.OAM);
			memory.canAccessOAM = false;
			blockStatIrqs = false;
		}

		public string GetPPUStateInfo(){
			return string.Format("LY = {0}",ly);
		}

		//TODO: Rewrite to be FIFO based/etc
		public void Step()
		{
			//TODO: properly emulate PPU behavior lol
			/*
			switch(mode){
				case PPUMode.HBlank:
				break;
				...
			}
			*/

			int mode0IntSelect = memory.regs.INTR_M0;
			int mode1IntSelect = memory.regs.INTR_M1;
			int mode2IntSelect = memory.regs.INTR_M2;
			int lycIntSelect = memory.regs.INTR_LYC;

			//Check if WY_latch should be enabled
			if(!WY_latch && memory.regs.windowEnable == 1 && ly == memory.regs.WY) WY_latch = true;

			//If we're not in vblank, check whether we should change mode
			if(mode != PPUMode.VBlank){
				//If 80 or more cycles have passed and we're in mode 2 (oam), change to mode 3 (drawing)
				if(scanlineCycleCount >= 80 && mode == PPUMode.OAM){
					OAMScan(); //Scan OAM to select up to 10 objects to be drawn.
					SetMode(PPUMode.Drawing);
				}

				//If we're done drawing the current scanline, change to mode 0 (hblank)
				//Drawing takes between 172-289 cycles, so for now let's assume it took the minimum.
				if(scanlineCycleCount >= 252 && mode == PPUMode.Drawing){
					DrawScanline(); //Draw the current scanline
					SetMode(PPUMode.HBlank);
					//If the mode 0 condition was enabled and we changed to mode 0, request a stat interrupt
					if(mode0IntSelect == 1) RequestLCDInterrupt();

					//If a VRAM HBlank DMA transfer is happening, transfer over 0x10 bytes
					//TODO: this shouldn't be immediate
					if(memory.doingVramDMATransfer && memory.regs.vramDMAMode == 1){
						for(int i = 0; i < 16; i++){
							memory.VRAMDMAStep();
						}
					}
				}
			}
			
			//If 456 or more cycles have passed, go to the next scanline
			if(scanlineCycleCount >= cyclesPerScanline){
				scanlineCycleCount %= cyclesPerScanline;
				
				ly++;
				if(memory.regs.windowEnable == 1){
					if(WY_latch && memory.regs.WX <= 166){
						windowLine++;
					}
				}

				if(ly < 144){
					SetMode(PPUMode.OAM);
				}else if(ly == 144){
					//If ly is 144, we're at the start of vblank
					SetMode(PPUMode.VBlank);
					//Request a VBlank interrupt
					memory.RequestInterrupt(Interrupt.VBlank);
					//If the mode 1 condition was enabled and we changed to mode 1, request a stat interrupt
					if(mode1IntSelect == 1) RequestLCDInterrupt();

					//If the LCD was turned on again this frame, discard the frame
					if(lcdReenabled){
						display.Clear();
						lcdReenabled = false;
					}
				}else if(ly == 154){
					//If ly is 154, we're at the end of vblank, reset ly to 0
					ly = 0;
					WY_latch = false;
					finishedFrame = true;
					windowLine = 0;
					SetMode(PPUMode.OAM);
				}
				memory.regs.LY = ly;

				//If the mode 2 condition is enabled and we changed to mode 2, request a stat interrupt
				if(mode == PPUMode.OAM && mode2IntSelect == 1){
					RequestLCDInterrupt();
				}

			}

			int lyc = memory.regs.LYC;
			int lycFlag = ly == lyc ? 1 : 0;

			memory.regs.LYC_STAT = lycFlag;

			//If the lyc condition is enabled and ly == lyc, request a stat interrupt
			if(lycFlag == 1 && lycIntSelect == 1){
				RequestLCDInterrupt();
			}

			//If none of the conditions for a stat irq are met, disable stat irq blocking
			if(!(mode0IntSelect == 1 && mode == PPUMode.HBlank) && !(mode1IntSelect == 1 && mode == PPUMode.VBlank)
			&& !(mode2IntSelect == 1 && mode == PPUMode.OAM) && !(lycIntSelect == 1 && lycFlag == 1)){
				blockStatIrqs = false;
			}

			//Increment the current scanline cycle count
			scanlineCycleCount++;
		}

		void SetMode(PPUMode newMode){
			mode = newMode;
			int modeVal = (int)mode;
			//Update the mode value in STAT (bits 0-1)
			memory.regs.LCD_MODE = modeVal;

			//Update the memory access flags depending on the new mode
			//Disabled for now until accuracy improves
			
			if(newMode == PPUMode.OAM){
				memory.canAccessOAM = false;
			}else if(newMode == PPUMode.Drawing){
				memory.canAccessVRAM = false;
			}else if(newMode == PPUMode.HBlank || newMode == PPUMode.VBlank){
				memory.canAccessOAM = true;
				memory.canAccessVRAM = true;
			}
			
		}


		List<ObjectAttributes> scanlineObjects = new List<ObjectAttributes>();

		/*
		Scans through OAM to find up to 10 objects to render on the current scanline,
		and sorts them based on priority.
		Priority works as follows:
		On Game Boy only, object priority is determined first by x position, with the leftmost objects
		having highest priority, and then by OAM order.
		On Game Boy Color, priority is determined solely by OAM order.
		*/
		void OAMScan(){
			int height = memory.regs.objSize == 0 ? 8 : 16;

			scanlineObjects.Clear();

			//Loop through each OAM entry, and find up to 10 objects which are
			//on the current scanline, regardless of whether they're on screen or not.
			foreach(ObjectAttributes obj in memory.oam){
				int yPos = obj.y - 16;

				//If the object is on the scanline, add it to the list.
				if(ly >= yPos && ly < yPos + height){
					scanlineObjects.Add(obj);
				}

				//If we now have chosen 10 objects, exit the loop
				if(scanlineObjects.Count == 10) break;
			}

			/*
			If on Game Boy, we need to sort the array by x position while maintaining
			OAM order. Array.Sort isn't a stable sort, so Enumrable.OrderBy is used instead.
			*/
			scanlineObjects = scanlineObjects.OrderBy(obj => obj.x).ToList();
		}

		void RequestLCDInterrupt(){
			if(!blockStatIrqs){
				blockStatIrqs = true;
				memory.RequestInterrupt(Interrupt.LCD);
			}
		}

		//Updates the gameboy palette arrays from the values currently stored in
		//the 3 palette registers.
		void UpdateGBPaletteArrays(){
			byte bgp = memory.regs.BGP;
			byte obp0 = memory.regs.OBP0;
			byte obp1 = memory.regs.OBP1;

			for(int i = 0; i < 4; i++){
				bgPalette[i] = (byte)((bgp >> (i*2)) & 3);
				objPalette0[i] = (byte)((obp0 >> (i*2)) & 3);
				objPalette1[i] = (byte)((obp1 >> (i*2)) & 3);
			}

		}

		int currentPixelBGPaletteIndex; //used to keep track of the current pixel's bg palette index

		//Draws the current scanline.
		void DrawScanline(){
			//Update the palettes from the current palette register values (BGP, OBP0, OBP1)
			UpdateGBPaletteArrays();

			byte bgp = memory.regs.BGP;
			int tileDataArea = memory.regs.bgWindowTileDataArea;
			int tempBgTilemapArea = memory.regs.bgTilemapArea;
			int tempWindowTilemapArea = memory.regs.windowTilemapArea;

			int scrollX = memory.regs.SCX;
			int scrollY = memory.regs.SCY;
			int windowX = memory.regs.WX - 7;
			int windowY = memory.regs.WY;
			int windowStartX = windowX >= 0 ? windowX : 0;
			bool windowVisible = WY_latch && windowX < 160;

			for(int x = 0; x < 160; x++){
				int y = ly;
				currentPixelBGPaletteIndex = 0;
				//Check if the bg/window are enabled
				if(memory.regs.bgWindowEnablePriority == 1){
					//If so, draw the background for the current pixel
					int tilemapPixelXPos = (x + scrollX) % 256;
					int tilemapPixelYPos = (y + scrollY) % 256;
					DrawBGWindowTilePixel(x, y, tilemapPixelXPos, tilemapPixelYPos, tempBgTilemapArea, tileDataArea);

					//If the window is enabled/visible and appears on this pixel, draw it
					if(memory.regs.windowEnable == 1 && windowVisible && x >= windowStartX){
						tilemapPixelXPos = x - windowX;
						tilemapPixelYPos = windowLine;
						DrawBGWindowTilePixel(x, y, tilemapPixelXPos, tilemapPixelYPos, tempWindowTilemapArea, tileDataArea);
					}
				}

				if(memory.regs.objEnable == 1){
					DrawObjectPixel(x,y);
				}
			}
		}

		void DrawBGWindowTilePixel(int x, int y, int tilemapX, int tilemapY, int tilemapArea, int tileDataArea){
			int tilemapStartAddress = tilemapArea == 0 ? 0x9800 : 0x9C00;
			int tileDataStartAddress = tileDataArea == 1 ? 0x8000 : 0x9000;
			int tileX = tilemapX/8;
			int tileY = tilemapY/8;
			int tilePixelXPos = tilemapX % 8;
			int tilePixelYPos = tilemapY % 8;
			int tilemapByteIndex = tileY*32 + tileX;
			int tileIndex = memory.vram[tilemapStartAddress + tilemapByteIndex - 0x8000];
			//If the tile data area is 0, the tile index is a signed byte (-128,127)
			if(tileDataArea == 0) tileIndex = (sbyte)tileIndex;

			int tileAddress = tileDataStartAddress + tileIndex*16;

			byte loByte = memory.vram[tileAddress + tilePixelYPos*2 - 0x8000];
			byte hiByte = memory.vram[tileAddress + tilePixelYPos*2 + 1 - 0x8000];
			int lo = (loByte >> (7-tilePixelXPos)) & 1;
			int hi = (hiByte >> (7-tilePixelXPos)) & 1;
			int palIndex = lo + (hi << 1);
			display.DrawGBPixel(x,y,bgPalette[palIndex]);
			currentPixelBGPaletteIndex = palIndex;
		}

		void DrawObjectPixel(int x, int y){
			int size = memory.regs.objSize;

			//Iterate through the objects from highest to lowest priority and choose the first
			//object that appears on this pixel.
			for(int i = 0; i < scanlineObjects.Count; i++){
				ObjectAttributes obj = scanlineObjects[i];
				int tileIndex = obj.tileIndex;
				int screenX = obj.x - 8;
				int screenY = obj.y - 16;
				int width = 8;
				int height = size == 0 ? 8 : 16;

				//If this object appears on this pixel, draw it for this pixel.
				if(x >= screenX && x < screenX + width){
					//Calculate which pixel from which tile needs to be drawn.
					int tileX = x - screenX;
					int tileY = 0;

					if(obj.xFlip) tileX = 7 - tileX;

					if(size == 0){
						tileY = y - screenY;
						if(obj.yFlip) tileY = 7 - tileY;
					}else if(size == 1){
						//For 8x16 sprite mode, bit 0 of the tile index is ignored.
						tileIndex &= 0b11111110;
						//If the sprite mode is 8x16, check whether a line from the top or bottom half should be rendered.
						tileY = y - screenY;
						if(obj.yFlip) tileY = 15 - tileY;
						//If the bottom half is on the scanline, increment the tile index.
						if(tileY >= 8){
							tileIndex++;
							tileY %= 8;
						}
					}

					//Draw the pixel.
					int tileAddress = tileIndex*16;

					byte loByte = memory.vram[tileAddress + tileY*2];
					byte hiByte = memory.vram[tileAddress + tileY*2 + 1];

					int lo = (loByte >> (7-tileX)) & 1;
					int hi = (hiByte >> (7-tileX)) & 1;
					int palIndex = lo + (hi << 1);

					//If the pixel has a palette index of 0 (transparent), don't draw it
					if(palIndex == 0) continue;

					byte color = obj.palette == 0 ? objPalette0[palIndex] : objPalette1[palIndex];

					//If the object's priority is 1, and the bg/window pixel color isn't 0,
					//don't render this pixel
					if(obj.priority == 1 && currentPixelBGPaletteIndex != 0) break;
					display.DrawGBPixel(x,y,color);
				
					break;
				}
			}
		}

	}
}

