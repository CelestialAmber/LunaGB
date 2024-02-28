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
		int tileWidth = 16;
		int tileHeight;
		LunaImage tileDataImage;

		public GraphicsViewer(Emulator emulator)
		{
			InitializeComponent();

			memory = emulator.GetMemory();
			UpdateTileDataImage();
			UpdateGraphicsView();
		}

		private void OnHitUpdateButton(object sender, RoutedEventArgs e){
			UpdateGraphicsView();
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
		}


		Color[] palette = {new Color(255,255,255), new Color(170,170,170), new Color(85,85,85), new Color(0,0,0)};


		public void ReadVRAM(){
			for(int i = 0; i < 0x1000; i++){
				tileData[i] = memory.GetByte(0x8000 + i);
			}

			for(int i = 0; i < 0x800; i++){
				tilemapData[i] = memory.GetByte(0x9800 + i);
			}
		}

		public void DrawTileData(){
			int tiles = tileData.Length/16;
			int tileIndex = 0;
			
			for(int y = 0; y < tileHeight; y++){
				for(int x = 0; x < tileWidth; x++){
					DrawTile(x,y,tileIndex);
					tileIndex++;
					if(tileIndex == tiles) break;
				}
			}

			UpdateTileDataDisplay();
		}

		public void UpdateTileDataDisplay() {
			tileDataImageBox.Source = new Bitmap(new MemoryStream(tileDataImage.ToByteArray()));
		}

		void DrawTile(int xPos, int yPos, int tileIndex){
			int tileDataOffset = tileIndex*16;

			for(int y = 0; y < 8; y++){
				byte loByte = tileData[tileDataOffset + y*2];
				byte hiByte = tileData[tileDataOffset + y*2 + 1];
				for(int x = 0; x < 8; x++){
					int lo = (loByte >> (7-x)) & 1;
					int hi = (hiByte >> (7-x)) & 1;
					int palIndex = lo + (hi << 1);
					Color col = palette[palIndex];
					tileDataImage.SetPixel(xPos*8 + x,yPos*8 + y, col);
				}
			}
		}

	}
}