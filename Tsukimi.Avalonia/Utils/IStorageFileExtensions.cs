using Avalonia.Platform.Storage;
using System.Net;

namespace Tsukimi.Avalonia.Utils {
	public static class IStorageFileExtensions {
		//Because Avalonia is dumb ;<
		public static string ConvertPathToString(this IStorageFile file){
			return WebUtility.UrlDecode(file.Path.AbsolutePath);
		}

		public static string ConvertPathToString(this IStorageItem file){
			return WebUtility.UrlDecode(file.Path.AbsolutePath);
		}
	}
}