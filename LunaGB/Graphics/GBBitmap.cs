using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LunaGB.Graphics
{
    public class GBBitmap
    {
        const int width = 160;
        const int height = 144;
        Color[,] pixels = new Color[width, height];

        public GBBitmap()
        {
            pixels = new Color[width, height];
        }

        public void SetPixel(int x, int y, Color color) {
            pixels[x, y] = color;
        }

        public void GetPixel(int x, int y, Color color) {
            pixels[x, y] = color;
        }

        public byte[] ToByteArray() {
            Image<Rgba32> image = new Image<Rgba32>(width, height);

            for(int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Color col = pixels[x, y];
                    image[x, y] = new Rgba32((byte)col.r, (byte)col.g, (byte)col.b);
                }
            }

            MemoryStream ms = new MemoryStream();
            image.SaveAsBmp(ms);
            image.Dispose();
            return ms.ToArray();
        }
    }
}

