﻿using System;
using System.Data.Common;
using System.Collections.Generic;
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
		Display display;

		public byte ly = 0;
		int windowLine = 0; //Internal line counter for the window
		PPUMode mode;
		public const int cyclesPerScanline = 456; //each scanline takes 456 cycles
		public int scanlineCycleCount = 0;
		bool blockStatIrqs = false;
		byte[,] pixels = new byte[160,144]; //used to store pixels before rendering the final image to the screen


		//PPU registers

		byte BGP {
			get{
				return memory.hram[(int)IORegister.BGP];
			}
		}

		byte OBP0 {
			get{
				return memory.hram[(int)IORegister.OBP0];
			}
		}

		byte OBP1 {
			get{
				return memory.hram[(int)IORegister.OBP1];
			}
		}

		//LCDC flags

		//BG/Window enable/priority (bit 0)
		public int bgWindowEnablePriority {
			get{
				return memory.GetHRAMBit(0, (int)IORegister.LCDC);
			}
		}

		//OBJ enable (bit 1)
		public int objEnable {
			get{
				return memory.GetHRAMBit(1, (int)IORegister.LCDC);
			}
		}

		//OBJ size (bit 2)
		public int objSize {
			get{
				return memory.GetHRAMBit(2, (int)IORegister.LCDC);
			}
		}

		//BG Tilemap Area (bit 3)
		public int bgTilemapArea {
			get{
				return memory.GetHRAMBit(3, (int)IORegister.LCDC);
			}
		}

		//BG/Window tile data area (bit 4)
		public int bgWindowTileDataArea {
			get{
				return memory.GetHRAMBit(4, (int)IORegister.LCDC);
			}
		}

		//Window enable (bit 5)
		public int windowEnable {
			get{
				return memory.GetHRAMBit(5, (int)IORegister.LCDC);
			}
		}

		//Window tilemap area (bit 6)
		public int windowTilemapArea {
			get{
				return memory.GetHRAMBit(6, (int)IORegister.LCDC);
			}
		}

		//LCD enable (bit 7)
		public int lcdcEnable {
			get{
				return memory.GetHRAMBit(7, (int)IORegister.LCDC);
			}
		}

		public int WX {
			get{
				return memory.GetIOReg(IORegister.WX);
			}
		}

		public int WY {
			get{
				return memory.GetIOReg(IORegister.WY);
			}
		}


		//Arrays for storing the GB register color palettes for easier use
		byte[] bgPalette = new byte[4];
		byte[] objPalette0 = new byte[4];
		byte[] objPalette1 = new byte[4];

		//used to store the indices of the objects selected to be drawn
		List<int> scanlineObjIndices = new List<int>();

		public PPU(Memory memory, Display display)
		{
			this.display = display;
			this.memory = memory;
			
		}

		public void Init(){
			ly = 0;
			windowLine = 0;
			scanlineCycleCount = 0;
			SetMode(PPUMode.OAM);
			memory.canAccessOAM = false;
			blockStatIrqs = false;
			ClearPixelData();
		}

		//TODO:
		//-The cpu should really eventually be rewritten to just be cycle accurate
		//so the PPU can step cycle by cycle instead.
		//-Maybe move the PPU to its own thread?
		//-The different PPU actions should also actually be done during the respective modes instead
		//of faking it.
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

			byte stat = memory.GetIOReg(IORegister.STAT);
			int mode0IntSelect = (stat >> 3) & 1;
			int mode1IntSelect = (stat >> 4) & 1;
			int mode2IntSelect = (stat >> 5) & 1;
			int lycIntSelect = (stat >> 6) & 1;

			//Increment the current scanline cycle count
			scanlineCycleCount++;

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
					if(mode0IntSelect == 1) RequestSTATInterrupt();
				}
			}
			
			//If 456 or more cycles have passed, go to the next scanline
			if(scanlineCycleCount >= cyclesPerScanline){
				scanlineCycleCount %= cyclesPerScanline;
				
				ly++;

				if(ly < 144){
					SetMode(PPUMode.OAM);
				}else if(ly == 144){
					//If ly is 144, we're at the start of vblank
					SetMode(PPUMode.VBlank);
					//Request a VBlank interrupt
					RequestVBlankInterrupt();
					//If the mode 1 condition was enabled and we changed to mode 1, request a stat interrupt
					if(mode1IntSelect == 1) RequestSTATInterrupt();
					//Update the display
					UpdateDisplay();
					windowLine = 0;
				}else if(ly == 154){
					//If ly is 154, we're at the end of vblank, reset ly to 0
					ly = 0;
					SetMode(PPUMode.OAM);
				}
				memory.SetIOReg(IORegister.LY, ly);

				//If the mode 2 condition is enabled and we changed to mode 2, request a stat interrupt
				if(mode == PPUMode.OAM && mode2IntSelect == 1){
					RequestSTATInterrupt();
				}

			}

			int lyc = memory.GetIOReg(IORegister.LYC);
			int lycFlag = ly == lyc ? 1 : 0;

			memory.SetHRAMBit((int)IORegister.STAT, 2, lycFlag);

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
			memory.SetHRAMBit((int)IORegister.STAT,0,modeVal & 1);
			memory.SetHRAMBit((int)IORegister.STAT,0,(modeVal >> 1) & 1);

			//Update the memory access flags depending on the new mode
			/*
			if(newMode == PPUMode.OAM){
				memory.canAccessOAM = false;
			}else if(newMode == PPUMode.Drawing){
				memory.canAccessVRAM = false;
			}else if(newMode == PPUMode.HBlank || newMode == PPUMode.VBlank){
				memory.canAccessOAM = true;
				memory.canAccessVRAM = true;
			}
			*/
		}


		/* TODO: on gb, the indices should be sorted by x position from right to left to correctly
		emulate priority. */
		void OAMScan(){
			int height = objSize == 0 ? 8 : 16;

			scanlineObjIndices.Clear(); //Clear the object list

			//Loop through each OAM entry, and find up to 10 objects which are
			//on the current scanline, regardless of whether they're on screen or not.
			for(int i = 0; i < 40; i++){
				int yPos = memory.oam[i*4] - 16;

				//If the object is on the scanline, add it to the list.
				if(ly >= yPos && ly < yPos + height){
					scanlineObjIndices.Add(i);
				}

				//If we now have chosen 10 objects, return
				if(scanlineObjIndices.Count == 10) return;
			}
		}

		void RequestVBlankInterrupt(){
			memory.SetHRAMBit((int)IORegister.IF, 0, 1);
		}

		void RequestSTATInterrupt(){
			if(!blockStatIrqs){
				blockStatIrqs = true;
				memory.SetHRAMBit((int)IORegister.IF, 1, 1);
			}
		}

		public void UpdateDisplay(){
			//Update the display with the raw pixel data
			display.Update(pixels);
			//Reset the pixels to 0
			ClearPixelData();
		}


		void ClearPixelData(){
			for(int x = 0; x < 160; x++){
				for(int y = 0; y < 144; y++){
					pixels[x,y] = 0;
				}
			}
		}

		//Updates the gameboy palette arrays from the values currently stored in
		//the 3 palette registers.
		void UpdateGBPaletteArrays(){
			byte bgp = BGP;
			byte obp0 = OBP0;
			byte obp1 = OBP1;

			for(int i = 0; i < 4; i++){
				bgPalette[i] = (byte)((bgp >> (i*2)) & 3);
				objPalette0[i] = (byte)((obp0 >> (i*2)) & 3);
				objPalette1[i] = (byte)((obp1 >> (i*2)) & 3);
			}

		}

		//Draws the current scanline.
		void DrawScanline(){
			//Update the palettes from the current palette register values (BGP, OBP0, OBP1)
			UpdateGBPaletteArrays();
			//Check if the bg/window enable flag is enabled
			if(bgWindowEnablePriority == 1){
				//If so, draw the background
				DrawBackground();
				//If the window enable flag is also enabled, draw the window
				if(windowEnable == 1){
					DrawWindow();
				}
			}
			//Draw all objects on this scanline if objects are enabled
			if(objEnable == 1){
				DrawObjects();
			}
		}

		void DrawBackground(){
			byte bgp = BGP;
			int tileDataArea = bgWindowTileDataArea;
			int tilemapArea = bgTilemapArea;
			int tileDataStartAddress = tileDataArea == 1 ? 0x8000 : 0x9000;
			int tilemapStartAddress = tilemapArea == 0 ? 0x9800 : 0x9C00;

			int scrollX = memory.GetIOReg(IORegister.SCX);
			int scrollY = memory.GetIOReg(IORegister.SCY);

			for(int x = 0; x < 160; x++){
				int y = ly;
				int tilemapPixelXPos = (x + scrollX) % 256;
				int tilemapPixelYPos = (y + scrollY) % 256;
				int tileX = tilemapPixelXPos/8;
				int tileY = tilemapPixelYPos/8;
				int tilePixelXPos = tilemapPixelXPos % 8;
				int tilePixelYPos = tilemapPixelYPos % 8;
				int tilemapByteIndex = tileY*32 + tileX;
				int tileIndex = memory.GetByte(tilemapStartAddress + tilemapByteIndex);
				//If the tile data area is 0, the tile index is a signed byte (-128,127)
				if(tileDataArea == 0) tileIndex = (sbyte)tileIndex;

				int tileAddress = tileDataStartAddress + tileIndex*16;

				byte loByte = memory.GetByte(tileAddress + tilePixelYPos*2);
				byte hiByte = memory.GetByte(tileAddress + tilePixelYPos*2 + 1);
				int lo = (loByte >> (7-tilePixelXPos)) & 1;
				int hi = (hiByte >> (7-tilePixelXPos)) & 1;
				int palIndex = lo + (hi << 1);
				pixels[x,y] = bgPalette[palIndex];
			}
		}

		void DrawWindow(){
			byte bgp = BGP;
			int tileDataArea = bgWindowTileDataArea;
			int tilemapArea = windowTilemapArea;
			int tileDataStartAddress = tileDataArea == 1 ? 0x8000 : 0x9000;
			int tilemapStartAddress = tilemapArea == 0 ? 0x9800 : 0x9C00;
			int windowX = WX - 7;
			int windowY = WY;

			//If the window isn't visible, don't render it
			if(windowX >= 160 || windowY >= 144 || (ly < windowY)) return;

			int startX = windowX >= 0 ? windowX : 0;

			for(int x = startX; x < 160; x++){
				int y = windowLine;
				int tilemapPixelXPos = x - windowX;
				int tilemapPixelYPos = y - windowY;
				int tileX = tilemapPixelXPos/8;
				int tileY = tilemapPixelYPos/8;
				int tilePixelXPos = tilemapPixelXPos % 8;
				int tilePixelYPos = tilemapPixelYPos % 8;
				int tilemapByteIndex = tileY*32 + tileX;
				int tileIndex = memory.GetByte(tilemapStartAddress + tilemapByteIndex);
				//If the tile data area is 0, the tile index is a signed byte (-128,127)
				if(tileDataArea == 0) tileIndex = (sbyte)tileIndex;

				int tileAddress = tileDataStartAddress + tileIndex*16;

				byte loByte = memory.GetByte(tileAddress + tilePixelYPos*2);
				byte hiByte = memory.GetByte(tileAddress + tilePixelYPos*2 + 1);
				int lo = (loByte >> (7-tilePixelXPos)) & 1;
				int hi = (hiByte >> (7-tilePixelXPos)) & 1;
				int palIndex = lo + (hi << 1);
				pixels[x,y] = bgPalette[palIndex];
			}

			windowLine++;
		}

		/* Loops through each object selected to be drawn on the current scanline,
		and draws all visible ones. */
		void DrawObjects(){
			byte lcdc = memory.GetIOReg(IORegister.LCDC);
			int size = objSize;

			for(int i = 0; i < scanlineObjIndices.Count; i++){
				int objIndex = scanlineObjIndices[i];
				int yPos = memory.oam[objIndex*4];
				int xPos = memory.oam[objIndex*4 + 1];

				int tileIndex = memory.oam[objIndex*4 + 2];
				/*
				Flags:
				bits 0-2: cgb palette (gbc only)
				bit 3: vram bank (gbc only)
				bit 4: dmg palette (gb only)
				bit 5: x flip
				bit 6: y flip
				bit 7: priority
				*/
				byte flags = memory.oam[objIndex*4 + 3];
				int palette = (flags >> 4) & 1;
				bool xFlip = ((flags >> 5) & 1) == 1 ? true : false;
				bool yFlip = ((flags >> 6) & 1) == 1 ? true : false;
				int priority = (flags >> 7) & 1;
				
				//Only render the object if part of it will show up on screen
				if(xPos > 0 && xPos < 168){
					int screenX = xPos - 8;
					int screenY = yPos - 16;
					//Next, determine which line of the object
					//should be rendered.
					int height = size == 0 ? 8 : 16;
					int lineToRender = 0;

					if(size == 0){
						lineToRender = ly - screenY;
						if(yFlip) lineToRender = 7 - lineToRender;
					}else if(size == 1){
						//If the sprite mode is 8x16, check whether a line from the top or bottom half should be rendered.
						lineToRender = ly - screenY;
						if(yFlip) lineToRender = 15 - lineToRender;
						//If the bottom half is on the scanline, increment the tile index.
						if(lineToRender >= 8){
							tileIndex = (tileIndex & 0b11111110) + 1;
							lineToRender %= 8;
						}
					}

					DrawObjectLine(screenX, lineToRender, xFlip, priority, palette, tileIndex);
				}
			}
		}


		void DrawObjectLine(int xPos, int y, bool xFlip, int priority, int palette, int tileIndex){
			int tileAddress = 0x8000 + tileIndex*16;

			byte loByte = memory.GetByte(tileAddress + y*2);
			byte hiByte = memory.GetByte(tileAddress + y*2 + 1);
			for(int x = 0; x < 8; x++){
				int lo = (loByte >> (7-x)) & 1;
				int hi = (hiByte >> (7-x)) & 1;
				int palIndex = lo + (hi << 1);

				//If the object's priority flag is 1 and this pixel's color index isn't 0,
				//don't render it
				if(palIndex == 0) continue;

				byte color = palette == 0 ? objPalette0[palIndex] : objPalette1[palIndex];

				int pixelX = xPos + (xFlip ? 7-x : x);
				int pixelY = ly;
				//Only render the current pixel if it is within the screen
				if(pixelX >= 0 && pixelX < 160){
					//If the object's priority is 1, and the bg/window pixel color isn't 0,
					//don't render this pixel
					if(priority == 1 && pixels[pixelX, pixelY] != 0) continue;
					pixels[pixelX,pixelY] = color;
				}
			}
		}

	}
}

