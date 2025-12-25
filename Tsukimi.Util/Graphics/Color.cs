using System;

namespace Tsukimi.Graphics
{
    public class Color
    {
        public int r = 0, g = 0, b = 0, a = 255;

		public static Color black = new Color(0,0,0);
		public static Color white = new Color(255,255,255);

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

        public static Color CreateRandomColor()
        {
            Random rand = new Random();
            int r = rand.Next();
            int g = rand.Next();
            int b = rand.Next();
            return new Color(r, g, b);
        }

		public static Color operator * (Color col, float scale){
			return new Color((int)(col.r * scale), (int)(col.g * scale), (int)(col.b * scale));
		}

		public static Color operator / (Color col, float scale){
			return new Color((int)(col.r / scale), (int)(col.g / scale), (int)(col.b / scale));
		}
    }
}

