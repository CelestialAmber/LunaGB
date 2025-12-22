using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsukimi.Core.LunaGB;
using Tsukimi.Graphics;

namespace Tsukimi.Core
{


	//TODO: look into how to force variables to be implemented
	public abstract class Emulator
	{
		//These should maybe be put into a separate class?
		public const string name = "";
		public const int screenWidth = 0;
		public const int screenHeight = 0;
		public const EmulatorSystem system = EmulatorSystem.None;

		public Display display;

		public bool isRunning = false;
		public bool paused = false;

		public bool debug = false;
		public bool finishedFrame = false;
		public CancellationToken ctoken;

		public abstract bool LoadedRom();
		public abstract void LoadFile(string romPath);
		public abstract void Start(CancellationToken token);
		public abstract void DoSingleStep();
		public abstract void TogglePause();
		public abstract void Stop();
		public abstract LunaImage GetScreenBitmap();

		public Emulator()
		{
			//remove later
			display = new Display(0, 0);
		}
	}

}
