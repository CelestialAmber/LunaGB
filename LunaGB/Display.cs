using System;
using Tsukimi.Graphics;

namespace LunaGB {

public class Display
{

	public LunaImage display;
	public bool enabled = true;
	public int width;
	public int height;

	public delegate void OnRenderEvent();
	public event OnRenderEvent? OnRender;

	public Display(int width, int height){
		display = new LunaImage(width,height);
		this.width = width;
		this.height = height;
	}

	/*
	The Game Boy maps the color indices to colors like this:
	0: White
	1: Light Gray
	2: Dark Gray
	3: Black
	*/
	Color[] palette = {new Color(255,255,255), new Color(170,170,170), new Color(85,85,85), new Color(0,0,0)};

	//Draws a GB format pixel to the screen (palette index)
	public void DrawGBPixel(int x, int y, int pixel){
		Color col = palette[pixel];
		display.SetPixel(x,y, col);
	}

	//Draws a GBC format pixel to the screen (RGB555 color)
	public void DrawGBCPixel(int x, int y, int pixel){
		//Decode RGB555
		int r = pixel & 0b11111;
		int g = (pixel >> 5) & 0b11111;
		int b = (pixel >> 10) & 0b11111;
		Color col = new Color(r,g,b);
		display.SetPixel(x,y,col);
	}

	//Clears the display image to white.
	public void Clear(){
		for(int x = 0; x < width; x++){
			for(int y = 0; y < height; y++){
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