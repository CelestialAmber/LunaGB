using System;

namespace Tsukimi.Utils {

public static class ByteArrayExtensions {
	public static byte[] ToByteArray(this long val){
		byte[] bytes = BitConverter.GetBytes(val);
		if(!BitConverter.IsLittleEndian){
			Array.Reverse(bytes);
		}
		return bytes;
	}

	public static byte[] ToByteArray(this int val){
		byte[] bytes = BitConverter.GetBytes(val);
		if(!BitConverter.IsLittleEndian){
			Array.Reverse(bytes);
		}
		return bytes;
	}
}

}
