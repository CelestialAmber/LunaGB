using System;

namespace Tsukimi.Utils
{
    public class BitUtils
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

        //Big endian
        public static uint GenerateBitmask(int start, int end)
        {
            //Check later how much this impacts performance
            if(start < 0 || start >= 32 || end < 0 || end >= 32)
            {
                throw new IndexOutOfRangeException("Error: start/end indices are out of range");
            }

            //End > start: length = bits at end + bits at start
            //Otherwise, length = bits between start and end
            int length = start > end ? (32 - start) + end + 1 : end - start + 1;
            int shift = 31 - end;
            uint mask = (uint)((1 << length) - 1);
            mask = RotateLeft(mask, shift);

            return mask;
        }

        public static int SignExtend(uint value, int size)
        {
            if (size > 32) throw new Exception("Error: size must be 32 or less");

            int endIndex = 32 - (size + 1);
            int msb = (int)(value >> (size - 1));
            
            //If the size is less than 32 and the msb isn't 0, set the mask to the right number of ones starting from the top bit
            uint signExtendMask = (size == 32 || msb == 0) ? 0 : GenerateBitmask(0, endIndex);
            //Combine with the mask, then cast to int
            return (int)(signExtendMask | value);
        }
    }
}
