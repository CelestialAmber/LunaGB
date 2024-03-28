using System;
using System.Diagnostics;
using LunaGB.Core;
using LunaGB.Core.Debug;

namespace AOTTest {
	public class Program{
		public static Emulator? emu;
		public static int totalFrameTime;
		public static int framesRendered;

		public static void Main(string[] args){
			emu = new Emulator(new LunaGB.Core.Debug.Debugger());
			emu.display.OnRender += PrintFrameTime;
			emu.LoadROM("pokeblue.gb");

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
				totalFrameTime += (int)emu.frameTime;
				framesRendered++;
			}
		}


	}
}