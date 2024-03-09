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
		int[] bgPalette = new int[4];
		int tileWidth = 16;
		int tileHeight;
		int tilemapIndex = 0;
		int tilesetIndex = 0;
		LunaImage tileDataImage;
		LunaImage tilemapImage;

		public GraphicsViewer(Emulator emulator)
		{
			InitializeComponent();

			memory = emulator.GetMemory();
			tilemapImage = new LunaImage(256,256);
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
			ReadVRAM();
			DrawTileData();
			DrawTilemap();
			UpdateWindowImages();
		}

		public void UpdateWindowImages() {
			tileDataImageBox.Source = new Bitmap(new MemoryStream(tileDataImage.ToByteArray()));
			tilemapImageBox.Source = new Bitmap(new MemoryStream(tilemapImage.ToByteArray()));
		}


		Color[] palette = {new Color(255,255,255), new Color(170,170,170), new Color(85,85,85), new Color(0,0,0)};


		public void ReadVRAM(){
			for(int i = 0; i < 0x1800; i++){
				tileData[i] = memory.GetByte(0x8000 + i);
			}

			for(int i = 0; i < 0x800; i++){
				tilemapData[i] = memory.GetByte(0x9800 + i);
			}
			byte bgp = memory.GetIOReg(IORegister.BGP);
			for(int i = 0; i < 4; i++){
				bgPalette[i] = (byte)((bgp >> (i*2)) & 3);
			}
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

	}
}