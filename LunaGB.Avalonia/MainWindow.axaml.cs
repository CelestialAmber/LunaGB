using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.Interactivity;
using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using LunaGB.Core;
using LunaGB.Graphics;
using LunaGB.Core.Debug;
using LunaGB.Avalonia.Views;

namespace LunaGB.Avalonia {
	public partial class MainWindow : Window {

		Emulator emulator;
		Debugger debugger;
		CancellationTokenSource cToken;
		Thread emuThread;

		MemoryViewer mv;
		GraphicsViewer graphicsViewer;

		//Keyboard button map
		public static Dictionary<Key,Input.Button> buttonKeys = new Dictionary<Key,Input.Button>{
			{ Key.Up,Input.Button.Up },
			{ Key.Down,Input.Button.Down },
			{ Key.Left,Input.Button.Left },
			{ Key.Right,Input.Button.Right },
			{ Key.RightShift,Input.Button.Select },
			{ Key.Enter,Input.Button.Start },
			{ Key.Z,Input.Button.B },
			{ Key.X,Input.Button.A },
		};

		public MainWindow() {
			InitializeComponent();
			debugger = new Debugger();
			emulator = new Emulator(debugger);
			KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;
			debugger.OnHitBreakpoint += OnHitBreakpoint;
			cToken = new CancellationTokenSource();
			emulator.display.OnRender += RenderFrame;

			DrawStartupImage();
		}

		protected virtual void OnClosing(CancelEventArgs e) {
			//If the emulator is still running, stop it before closing
			if(emulator.isRunning) {
				StopEmulation();
			}
		}

		public void DrawStartupImage(){
			LunaImage bitmap = emulator.GetScreenBitmap();
			for (int x = 0; x < 160; x++) {
				for (int y = 0; y < 144; y++) {
					if ((x + y) % 2 == 0) {
						bitmap.SetPixel(x, y, new Color((byte)x, (byte)y, 255));
					}
				}
			}
			
			UpdateDisplay(bitmap.ToByteArray());
		}

		//Called by the emulator thread to render the game screen on the window
		public void RenderFrame(){
			Dispatcher.UIThread.Post(() => UpdateDisplay(emulator.GetScreenBitmap().ToByteArray()));
		}

		public void UpdateDisplay(byte[] data) {
			ImageBox.Source = new Bitmap(new MemoryStream(data));
		}

		private async void LoadROM(object sender, RoutedEventArgs e) {
			if (emulator.isRunning)
			{
				emulator.isRunning = false;
				StopEmulation();
			}

			cToken = new CancellationTokenSource();

			//TODO: update this
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filters = new List<FileDialogFilter>();
			dialog.Title = "Open ROM file";
			FileDialogFilter gbFilter = new FileDialogFilter();
			gbFilter.Name = "GB/GBC";
			gbFilter.Extensions = new List<string>(new string[] { "gb", "gbc" });
			dialog.Filters.Add(gbFilter);

			 var result = await dialog.ShowAsync(this);


			if (result != null && result.Length != 0) {
				 string filename = result[0];
				 if (filename != null) {
					 string romName = filename;
					 Console.WriteLine("Loading \"" + romName + "\"");
					 emulator.LoadROM(romName);
					//If the ROM successfully loaded, start the emulator
					if(emulator.loadedRom) {
						emuThread = new Thread(() => emulator.Start(cToken.Token));
						emuThread.Start();
					}
				 }
			 }
		}


		//TODO: when the emulator isn't running, the stop button should be greyed out
		private void OnClickStopButton(object sender, RoutedEventArgs e)
		{
			if (emulator.isRunning)
			{
				StopEmulation();
			}
		}

		//TODO: make the pause button change to resume if already paused
		private void OnClickPauseButton(object sender, RoutedEventArgs e)
		{
			if (emulator.isRunning)
			{
				TogglePause();
			}
		}

		private void OnClickResetButton(object sender, RoutedEventArgs e)
		{
			if (emulator.isRunning)
			{
				Reset();
			}
		}

		private void OnClickStepButton(object sender, RoutedEventArgs e)
		{
			if (emulator.isRunning)
			{
				EmulatorStep();
			}
		}

		private void OpenMemoryViewerWindow(object sender, RoutedEventArgs e)
		{
			//If the memory viewer isn't open, open it
			if (mv == null || !mv.IsVisible)
			{
				mv = new MemoryViewer(emulator);
				mv.Show();
			}
		}

		private void OpenGraphicsViewerWindow(object sender, RoutedEventArgs e){
			//If the graphics viewer isn't open, open it
			if(graphicsViewer == null || !graphicsViewer.IsVisible){
				graphicsViewer = new GraphicsViewer(emulator);
				graphicsViewer.Show();
			}
		}

		private void Nyeh(object sender, RoutedEventArgs e) {
			Console.WriteLine("Nyeh...");
		}


		private void OnWindowSizeChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
			if (e.Property.Name.Equals("ClientSize", StringComparison.CurrentCulture)) {
				var imageBox = this.FindControl<Image>("ImageBox");
				if (imageBox != null) {
					imageBox.Width = Width;
					imageBox.Height = Height - 25;
				}
			}
		}

		public void UpdateDisplay(Bitmap image) {
			var imageBox = this.FindControl<Image>("ImageBox");
			if (imageBox != null) {
				imageBox.Source = image;
			}
		}

		public void TogglePause() {
			emulator.paused = !emulator.paused;

			if (emulator.paused) Console.WriteLine("Emulation paused");
			else Console.WriteLine("Emulation resumed");
		}

		public void Reset()
		{
			emulator.Stop();
			cToken.Cancel();
			cToken = new CancellationTokenSource();
			emuThread = new Thread(() => emulator.Start(cToken.Token));
			emuThread.Start();
			Console.WriteLine("Emulator reset");
		}

		public void StopEmulation() {
			emulator.Stop();
			cToken.Cancel();
			Console.WriteLine("Emulation stopped");
			DrawStartupImage();
		}

		public void EmulatorStep()
		{
			Console.WriteLine("Performing a single step");

			if (!emulator.paused) emulator.paused = true;
			emulator.DoSingleStep();
		}

		
		void OnHitBreakpoint(Breakpoint breakpoint)
		{
			Console.WriteLine("Breakpoint hit");
			emulator.PrintDebugInfo();
			emulator.paused = true;
			emulator.pausedOnBreakpoint = true;
		}




		private void OnKeyDown(object? sender, KeyEventArgs e) {
			if(buttonKeys.ContainsKey(e.Key)){
				Input.OnButtonDown(buttonKeys[e.Key]);
			}

			//Step
			if(e.Key == Key.D1 && emulator.isRunning)
			{
				EmulatorStep();
			}

			//Pause
			if (e.Key == Key.P && emulator.isRunning)
			{
				TogglePause();
			}

			//Toggle breakpoints
			if(e.Key == Key.D2)
			{
				debugger.breakpointsEnabled = !debugger.breakpointsEnabled;
				Console.WriteLine(debugger.breakpointsEnabled ? "Breakpoints enabled" : "Breakpoints disabled");
			}

			//Toggle debug messages
			if(e.Key == Key.D3)
			{
				emulator.debug = !emulator.debug;
				Console.WriteLine(emulator.debug ? "Debug messages enabled" : "Debug messages disabled");
			}
		}

		private void OnKeyUp(object? sender, KeyEventArgs e) {
			if(buttonKeys.ContainsKey(e.Key)) {
				Input.OnButtonUp(buttonKeys[e.Key]);
			}
		}

	}
}
