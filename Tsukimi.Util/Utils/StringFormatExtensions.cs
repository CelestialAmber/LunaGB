namespace Tsukimi.Utils
{
    public static class StringFormatExtensions
    {
        public static string ToHexString(this uint value)
        {
            return string.Format("0x{0:x}", value);
        }

        //Stupid
        public static string ToHexString(this int value)
        {
            return string.Format("{0}0x{1:x}", value < 0 ? "-" : "", value < 0 ? -value : value);
        }

        public static string ToHexString(this long value) {
            return string.Format("{0}0x{1:x}", value < 0 ? "-" : "", value < 0 ? -value : value);
        }
    }
}
