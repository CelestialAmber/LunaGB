using System;
using SkiaSharp;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LunaGB
{
    public static class Util
    {
        public static Bitmap ToAvaloniaBitmap(this Image bitmap) {
            using var memoryStream = new MemoryStream();
            bitmap.SaveAsBmp(memoryStream);
            bitmap.Dispose();
            return new Bitmap(memoryStream);
        }
    }
}

