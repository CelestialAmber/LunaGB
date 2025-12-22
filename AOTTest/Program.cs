using System;
using System.Diagnostics;
using Tsukimi.Core;
using Tsukimi.Core.LunaGB;

namespace AOTTest {
	public class Program{
		public static LunaGBEmulator? emu;
		public static int totalFrameTime;
		public static int framesRendered;

		public static void Main(string[] args){
			emu = new LunaGBEmulator(new Tsukimi.Debug.Debugger());
			emu.display.OnRender += PrintFrameTime;
			emu.LoadFile("pokeblue.gb");

			CancellationTokenSource ct = new CancellationTokenSource();
			Thread emuThread = new Thread(() => emu.Start(ct.Token));
			emuThread.Start();

			Stopwatch sw = new Stopwatch();
			sw.Start();

			while(sw.ElapsedMilliseconds < 5000){
			}

			sw.Stop();
			emu.Stop();
			ct.Cancel();

			Console.WriteLine("Average frame time: {0:0.00} ms", (float)totalFrameTime/(float)framesRendered);
		}

		public static void PrintFrameTime(){
			if(emu != null){
				//Console.WriteLine("Frame took {0} ms", emu.frameTime);
				totalFrameTime += (int)emu.averageFrameTime;
				framesRendered++;
			}
		}


	}
}