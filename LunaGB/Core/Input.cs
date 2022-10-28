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

        //Keyboard button map
        //public static Keys[] buttonKeys = { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.RightShift, Keys.Enter, Keys.Z, Keys.X };

        /* public static bool IsButtonPressed(Button button) {
             //return Keyboard.GetState().IsKeyDown(buttonKeys[(int)button]);
             return Keyboard.GetState().GetPressedKeyCount() > 0;
         }*/

    }
}

