namespace LunaGB.Core {

	//TODO: This maybe shouldn't be a static class?
	public class Options{
		public static double frameRate = 59.7275f; //frame rate (the exact rate is 59.727500569606 fps)
		public static bool limitFrameRate = true; //if false, framerate is uncapped
		public static GBSystem system = GBSystem.DMG; //system to emulate
		public static bool autoDetectSystem = true; //if enabled, the emulator automatically detects the system to use based on the loaded ROM.
		public static bool bootToPause = false;
	}

}