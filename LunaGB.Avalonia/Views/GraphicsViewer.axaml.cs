using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using LunaGB.Core;
using LunaGB.Graphics;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace LunaGB.Avalonia.Views
{
	public partial class GraphicsViewer : Window
	{
		Memory memory;
		byte[] tileData = new byte[0x1800];
		byte[] tilemapData = new byte[0x800];
		ObjectAttributes[] oam = new ObjectAttributes[0x40];
		int[] bgPalette = new int[4];
		int[] objPalette0 = new int[4];
		int[] objPalette1 = new int[4];
		int objectSize;
		int tileWidth = 16;
		int tileHeight;
		int tilemapIndex = 0;
		int tilesetIndex = 0;
		LunaImage tileDataImage;
		LunaImage tilemapImage;
		LunaImage objectsPreviewImage;

		public GraphicsViewer(Emulator emulator)
		{
			InitializeComponent();

			memory = emulator.GetMemory();
			tilemapImage = new LunaImage(256,256);
			objectsPreviewImage = new LunaImage(160,144);
			UpdateTileDataImage();
			UpdateGraphicsView();
		}

		private void OnClickUpdateButton(object sender, RoutedEventArgs e){
			UpdateSelectedTilemap();
			UpdateGraphicsView();
		}

		private void OnChangeTilemapIndexUpDownValue(object sender, NumericUpDownValueChangedEventArgs e){
			if(tilemapUpDown != null){
				UpdateSelectedTilemap();
				DrawTilemap();
				UpdateWindowImages();
			}
		}

		private void OnChangeTilesetIndexUpDownValue(object sender, NumericUpDownValueChangedEventArgs e){
			if(tilesetUpDown != null){
				UpdateSelectedTileset();
				DrawTilemap();
				UpdateWindowImages();
			}
		}

		public void UpdateSelectedTilemap(){
			if(tilemapUpDown.Value != null){
				tilemapIndex = (int)tilemapUpDown.Value;
			}
		}

		public void UpdateSelectedTileset(){
			if(tilesetUpDown.Value != null){
				tilesetIndex = (int)tilesetUpDown.Value;
			}
		}

		public void UpdateTileDataImage(){
			int tiles = tileData.Length/16;
			tileHeight = (int)Math.Ceiling((float)tiles/(float)tileWidth);
			int width = tileWidth*8;
			int height = tileHeight*8;
			tileDataImage = new LunaImage(width, height);
		}

		void UpdateGraphicsView(){
			ReadMemory();
			DrawTileData();
			DrawTilemap();
			DrawObjectPreview();
			UpdateWindowImages();
		}

		public void UpdateWindowImages() {
			tileDataImageBox.Source = new Bitmap(new MemoryStream(tileDataImage.ToByteArray()));
			tilemapImageBox.Source = new Bitmap(new MemoryStream(tilemapImage.ToByteArray()));
			objectPreviewItemBox.Source = new Bitmap(new MemoryStream(objectsPreviewImage.ToByteArray()));
		}


		Color[] palette = {new Color(255,255,255), new Color(170,170,170), new Color(85,85,85), new Color(0,0,0)};


		public void ReadMemory(){
			for(int i = 0; i < 0x1800; i++){
				tileData[i] = memory.vram[i];
			}

			for(int i = 0; i < 0x800; i++){
				tilemapData[i] = memory.vram[0x1800 + i];
			}

			for(int i = 0; i < 40; i++){
				oam[i] = memory.oam[i];
			}

			byte bgp = memory.regs.BGP;
			byte obp0 = memory.regs.OBP0;
			byte obp1 = memory.regs.OBP1;

			for(int i = 0; i < 4; i++){
				bgPalette[i] = (byte)((bgp >> (i*2)) & 3);
				objPalette0[i] = (byte)((obp0 >> (i*2)) & 3);
				objPalette1[i] = (byte)((obp1 >> (i*2)) & 3);
			}

			objectSize = memory.regs.objSize;
		}

		public void DrawTilemap(){
			int tilemapStartOffset = tilemapIndex == 0 ? 0 : 0x400;

			//Gameboy tilemaps are 32x32 tiles/256x256 pixels
			for(int y = 0; y < 32; y++){
				for(int x = 0; x < 32; x++){
					int offset = tilemapStartOffset + x + y*32;
					int tileIndex = tilesetIndex == 0 ? tilemapData[offset] : (sbyte)tilemapData[offset] + 256;
					DrawTile(x, y, tileIndex, tilemapImage);
				}
			}
		}

		public void DrawTileData(){
			int tiles = tileData.Length/16;
			int tileIndex = 0;
			
			for(int y = 0; y < tileHeight; y++){
				for(int x = 0; x < tileWidth; x++){
					DrawTile(x, y, tileIndex, tileDataImage);
					tileIndex++;
					if(tileIndex == tiles) break;
				}
			}
		}

		void DrawTile(int xPos, int yPos, int tileIndex, LunaImage image){
			int tileDataOffset = tileIndex*16;

			for(int y = 0; y < 8; y++){
				byte loByte = tileData[tileDataOffset + y*2];
				byte hiByte = tileData[tileDataOffset + y*2 + 1];
				for(int x = 0; x < 8; x++){
					int lo = (loByte >> (7-x)) & 1;
					int hi = (hiByte >> (7-x)) & 1;
					int palIndex = lo + (hi << 1);
					Color col = palette[palIndex];
					image.SetPixel(xPos*8 + x,yPos*8 + y, col);
				}
			}
		}

		void DrawObjectPreview(){
			//Fill the image with a base bg color
			for(int x = 0; x < 160; x++){
				for(int y = 0; y < 144; y++){
					objectsPreviewImage.SetPixel(x,y,new Color(255,0,255));
				}
			}

			int width = 8;
			int height = objectSize == 0 ? 8 : 16;

			//Draw all 40 objects
			for(int i = 0; i < 40; i++){
				ObjectAttributes obj = oam[i];

				int tileIndex = obj.tileIndex;
				int x = obj.x - 8;
				int y = obj.y - 16;
				bool xFlip = obj.xFlip;
				bool yFlip = obj.yFlip;
				int pal = obj.palette;

				//If the object is within the bounds of the screen, draw it to the preview image
				if(x + width >= 0 && x < 160 && y + height >= 0 && y < 144){
					if(objectSize == 0){
						DrawObjectTile(x,y,xFlip,yFlip,pal,tileIndex);
					}else{
						tileIndex &= 0b11111110; //Bit 0 is ignored for 8x16 mode
						DrawObjectTile(x,y,xFlip,yFlip,pal,yFlip ? tileIndex + 1 : tileIndex);
						DrawObjectTile(x,y + 8,xFlip,yFlip,pal,yFlip ? tileIndex : tileIndex + 1);
					}
				}
			}
		}

		void DrawObjectTile(int xPos, int yPos, bool xFlip, bool yFlip, int pal, int tileIndex){
			int tileAddress = tileIndex*16;

			for(int y = 0; y < 8; y++){
				byte loByte = tileData[tileAddress + y*2];
				byte hiByte = tileData[tileAddress + y*2 + 1];
				for(int x = 0; x < 8; x++){
					int lo = (loByte >> (7-x)) & 1;
					int hi = (hiByte >> (7-x)) & 1;
					int palIndex = lo + (hi << 1);

					//If the object's priority flag is 1 and this pixel's color index isn't 0,
					//don't render it
					if(palIndex == 0) continue;

					int color = pal == 0 ? objPalette0[palIndex] : objPalette1[palIndex];

					int pixelX = xPos + (xFlip ? 7-x : x);
					int pixelY = yPos + (yFlip ? 7-y : y);
					//Only render the current pixel if it is within the screen
					if(pixelX >= 0 && pixelX < 160 && pixelY >= 0 && pixelY < 144){
						objectsPreviewImage.SetPixel(pixelX, pixelY, palette[color]);
					}
				}
			}
		}

	}
}