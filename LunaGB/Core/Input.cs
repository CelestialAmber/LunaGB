using System;

namespace LunaGB.Core
{

    public class Input
    {
        public enum Button {
            Up,
            Down,
            Left,
            Right,
            Select,
            Start,
            B,
            A
        }

        //Button flag array
        public static bool[] buttonPressed = new bool[8];

        public static void OnButtonDown(Button button) {
            buttonPressed[(int)button] = true;
        }

		public static void OnButtonUp(Button button) {
			buttonPressed[(int)button] = false;
		}


	}
}

