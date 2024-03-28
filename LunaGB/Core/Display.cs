using System;
using LunaGB.Graphics;

namespace LunaGB.Core {

public class Display
{

	public LunaImage display;
	public bool enabled = true;

	public delegate void OnRenderEvent();
	public event OnRenderEvent? OnRender;

	public Display(){
		display = new LunaImage(160,144);
	}

	/*
	The Game Boy maps the color indices to colors like this:
	0: White
	1: Light Gray
	2: Dark Gray
	3: Black
	*/
	Color[] palette = {new Color(255,255,255), new Color(170,170,170), new Color(85,85,85), new Color(0,0,0)};


	//Updates the display image with the raw pixel data from the PPU.
	public void Update(byte[,] pixels){
		for(int x = 0; x < 160; x++){
			for(int y = 0; y < 144; y++){
				Color col = palette[pixels[x,y]];
				display.SetPixel(x,y, col);
			}
		}
	}

	public void DrawGBPixel(int x, int y, int pixel){
		Color col = palette[pixel];
		display.SetPixel(x,y, col);
	}

	//Clears the display image to white.
	public void Clear(){
		for(int x = 0; x < 160; x++){
			for(int y = 0; y < 144; y++){
				display.SetPixel(x,y,Color.white);
			}
		}
	}


	//Renders the image to the screen.
	public void Render(){
		OnRender?.Invoke();
	}

}

}