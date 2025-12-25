using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.Interactivity;
using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Tsukimi.Avalonia.Views;
using System.Timers;
using Avalonia.Platform.Storage;
using Tsukimi.Avalonia.Utils;
using Tsukimi.Core;
using Tsukimi.Graphics;
using Tsukimi.Debug;
using LunaGB;

namespace Tsukimi.Avalonia {
	public partial class MainWindow : Window {

		LunaGBEmulator emulator;
		Debugger debugger;
		CancellationTokenSource cToken;
		Thread emuThread;
		MemoryViewer mv;
		GBGraphicsViewer graphicsViewer; //This should be moved elsewhere as part of being more platform agnostic
		System.Timers.Timer updateFPSTimer;


		public MainWindow() {
			InitializeComponent();
			debugger = new Debugger();
			emulator = new LunaGBEmulator(debugger);
			displayView.KeyDown += OnKeyDown;
			displayView.KeyUp += OnKeyUp;
			debugger.OnHitBreakpoint += OnHitBreakpoint;
			cToken = new CancellationTokenSource();
			emulator.display.OnRender += RenderFrame;
			displayView.AddHandler(DragDrop.DropEvent, OnDragDropFile);
			InitUpdateFPSTimer();
			ClearScreen();
		}

		protected override void OnClosing(WindowClosingEventArgs e) {
			//If the emulator is still running, stop it before closing
			if(emulator.isRunning) {
				StopEmulation();
			}
			base.OnClosing(e);
		}

		void InitUpdateFPSTimer(){
			updateFPSTimer = new System.Timers.Timer(1000);
			updateFPSTimer.Elapsed += OnUpdateFPSTimerElapsed;
			updateFPSTimer.AutoReset = true;
		}

		void ClearScreen(){
			LunaImage bitmap = emulator.GetScreenBitmap();
			for (int x = 0; x < 160; x++) {
				for (int y = 0; y < 144; y++) {
					bitmap.SetPixel(x,y,Color.black);
				}
			}

			displayView.UpdateDisplay(bitmap);
		}

		//Makes the game screen dimmer and tinted purple for when emulation is stopped.
		void StopEmulationScreenEffect(){
			LunaImage bitmap = emulator.GetScreenBitmap();
			Random rand = new Random();
			for (int x = 0; x < 160; x++) {
				for (int y = 0; y < 144; y++) {
					Color col = bitmap.GetPixel(x,y);
					col /= 4f;
					float noise = rand.NextSingle() - 0.5f;
					int r = (int)Math.Clamp(col.r*1.1f + noise*10f,0,255);
					int g = (int)Math.Clamp(col.g + noise*10f,0,255);
					int b = (int)Math.Clamp(col.b*1.4f + noise*10f,0,255);
					bitmap.SetPixel(x, y, new Color(r,g,b));
				}
			}

			displayView.UpdateDisplay(bitmap);
		}

		//Called by the emulator thread to render the game screen on the window
		public void RenderFrame(){
			Dispatcher.UIThread.Post(() => displayView.UpdateDisplay(emulator.GetScreenBitmap()));
		}

		private void OnUpdateFPSTimerElapsed(object? source, ElapsedEventArgs e){
			if(emulator.isRunning) UpdateStatusBar();
		}

		void UpdateStatusBar(){
			float fps = 1000f/emulator.averageFrameTime;
			int speedPercent = (int)Math.Round(100f*(fps/Options.frameRate));
			string text = string.Format("{0:0.00} FPS ({1}%) Average frame time: {2:0.00} ms", fps, speedPercent, emulator.averageFrameTime);
			Dispatcher.UIThread.Post(() => displayView.UpdateStatusBar(text));
		}

		public void OnDragDropFile(object? sender, DragEventArgs e){
			if (emulator.isRunning){
				emulator.isRunning = false;
				StopEmulation();
			}

			cToken = new CancellationTokenSource();

			IEnumerable<IStorageItem>? items = e.Data.GetFiles();
			if(items != null){
				IStorageItem fileItem = items.ToList()[0];
				string filename = fileItem.ConvertPathToString();
				LoadROM(filename);
			}else{
				Console.WriteLine("Dropped folder/url? Whatever you dropped it isn't supported :<");
			}
		}

		private async void OnClickLoadROMButton(object sender, RoutedEventArgs e) {
			if (emulator.isRunning){
				emulator.isRunning = false;
				StopEmulation();
			}

			cToken = new CancellationTokenSource();

			FilePickerOpenOptions options = new FilePickerOpenOptions();
			options.Title = "Open ROM file";
			options.AllowMultiple = false;

			var files = await this.StorageProvider.OpenFilePickerAsync(options);

			if (files.Count != 0) {
				string filename = files[0].ConvertPathToString();
				if (filename != null) {
					LoadROM(filename);
				}
			 }
		}

		void LoadROM(string filename){
			string romName = filename;
			Console.WriteLine("Loading \"" + romName + "\"");
			emulator.LoadFile(romName);
			//If the ROM successfully loaded, start the emulator
			if(emulator.LoadedRom()) {
				emuThread = new Thread(() => emulator.Start(cToken.Token));
				emuThread.Start();
				updateFPSTimer.Enabled = true;
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
				graphicsViewer = new GBGraphicsViewer(emulator);
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

		public void UpdatePauseButtonText(string text){
			pauseMenuItem.Header = text;
		}

		public void UpdateDisplay(Bitmap image) {
			var imageBox = this.FindControl<Image>("ImageBox");
			if (imageBox != null) {
				imageBox.Source = image;
			}
		}

		public void TogglePause() {
			emulator.TogglePause();
			if (emulator.paused){
				Console.WriteLine("Emulation paused");
				UpdatePauseButtonText("_Resume");
			}
			else{
				Console.WriteLine("Emulation resumed");
				UpdatePauseButtonText("_Pause");
			}
		}


		public void Reset()
		{
			emulator.Stop();
			UpdatePauseButtonText("_Pause");
			cToken.Cancel();
			cToken = new CancellationTokenSource();
			emuThread = new Thread(() => emulator.Start(cToken.Token));
			emuThread.Start();
			Console.WriteLine("Emulator reset");
		}

		public void StopEmulation() {
			emulator.Stop();
			UpdatePauseButtonText("_Pause");
			cToken.Cancel();
			updateFPSTimer.Enabled = false;
			Console.WriteLine("Emulation stopped");
			StopEmulationScreenEffect();
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
		}


		private void OnKeyDown(object? sender, KeyEventArgs e) {
			if(Config.buttonKeys.ContainsKey(e.Key)){
				Input.OnButtonDown(Config.buttonKeys[e.Key]);
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
			if(Config.buttonKeys.ContainsKey(e.Key)) {
				Input.OnButtonUp(Config.buttonKeys[e.Key]);
			}
		}

	}
}
