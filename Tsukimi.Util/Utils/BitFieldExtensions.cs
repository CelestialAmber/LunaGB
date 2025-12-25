namespace Tsukimi.Utils
{
    public static class BitFieldExtensions
    {
        //Returns the bits within the given range (both start/end inclusive).
        public static uint GetBits(this uint value, int start, int end, bool bigEndian = true)
        {
            int startIndex = !bigEndian ? 31 - end : start;
            int endIndex = !bigEndian ? 31 - start : end;
            
            //The start should come before the end
            if (startIndex > endIndex) return 0;

            uint mask = BitUtils.GenerateBitmask(startIndex, endIndex);
            return (value & mask) >> (31 - endIndex);
        }

        public static void SetBits(this ref uint value, int start, int end, uint bits, bool bigEndian = true)
        {
            int startIndex = !bigEndian ? 31 - end : start;
            int endIndex = !bigEndian ? 31 - start : end;

            uint mask = BitUtils.GenerateBitmask(startIndex, endIndex);
            value = (value & ~mask) | ((bits << (31 - endIndex)) & mask);
        }


    }
}
