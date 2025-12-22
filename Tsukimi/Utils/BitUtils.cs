using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsukimi.Utils
{
    internal class BitUtils
    {
        public static uint Rotate(uint val, int shift, int direction)
        {
            if (direction == 0) {
                //Left rotate
                return (val << shift) | (val >> (32 - shift));
            }
            else
            {
                //Right rotate
                return (val >> shift) | (val << (32 - shift));
            }
        }

        public static uint RotateLeft(uint val, int shift)
        {
            return Rotate(val, shift, 0);
        }

        public static uint RotateRight(uint val, int shift)
        {
            return Rotate(val, shift, 1);
        }

        public static uint GenerateBitmask(int start, int end)
        {
            //End > start: length = bits at end + bits at start
            //Otherwise, length = bits between start and end
            int length = start > end ? (32 - start) + end + 1 : end - start + 1;
            int shift = 31 - end;
            uint mask = (uint)((1 << length) - 1);
            mask = RotateLeft(mask, shift);

            return mask;
        }
    }
}
