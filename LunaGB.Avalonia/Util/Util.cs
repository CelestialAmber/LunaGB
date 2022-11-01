using System;
using System.IO;
using SkiaSharp;
using Avalonia.Media.Imaging;
using System.Drawing.Imaging;

namespace LunaGB.Avalonia.Util
{
    public static class Util
    {
        public static Bitmap ToAvaloniaBitmap(this System.Drawing.Bitmap bitmap) {
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream,ImageFormat.Png);
            memoryStream.Position = 0;
            return new Bitmap(memoryStream);
        }
    }
}

