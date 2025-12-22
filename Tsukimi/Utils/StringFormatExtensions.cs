namespace Tsukimi.Utils
{
    internal static class StringFormatExtensions
    {
        //Stupid
        public static string ToHexString(this int value)
        {
            return string.Format("{0}0x{1:x}", value < 0 ? "-" : "", value < 0 ? -value : value);
        }
    }
}
