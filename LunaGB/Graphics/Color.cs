using System;
namespace LunaGB.Graphics
{
    public struct Color
    {
        public int r = 0, g = 0, b = 0, a = 255;

        public Color(int r, int g, int b) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 255;
        }

        public Color(int r, int g, int b, int a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color()
        {
        }
    }
}

