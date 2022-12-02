using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using LunaGB.Core;
using LunaGB.Graphics;
using LunaGB.Core.Debug;

namespace LunaGB.Avalonia {
    public partial class MainWindow : Window {

        Emulator emulator;
        Image imageBox;
        CancellationTokenSource cToken;
        Thread emuThread;

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
            DataContext = this;
            AvaloniaXamlLoader.Load(this);
            imageBox = this.FindControl<Image>("ImageBox");
            emulator = new Emulator();
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Debugger.OnHitBreakpoint += OnHitBreakpoint;
			cToken = new CancellationTokenSource();

			GBBitmap bitmap = new GBBitmap();
            for (int x = 0; x < 160; x++) {
                for (int y = 0; y < 144; y++) {
                    if ((x + y) % 2 == 0) {
                        bitmap.SetPixel(x, y, new Color((byte)x, (byte)y, 255));
                    }
                }
            }
            UpdateDisplay(bitmap.ToByteArray());
        }

        protected override bool HandleClosing() {
            //If the emulator is still running, stop it before closing
            if (emulator.isRunning)
            {
                StopEmulation();
            }
            return base.HandleClosing();
        }

        public void UpdateDisplay(byte[] data) {
            imageBox.Source = new Bitmap(new MemoryStream(data));
        }

        public async void LoadROM() {
            if (emulator.isRunning)
            {
                emulator.isRunning = false;
                StopEmulation();
            }

			cToken = new CancellationTokenSource();

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
        public void OnClickStopButton()
        {
            if (emulator.isRunning)
            {
                StopEmulation();
            }
        }

        //TODO: make the pause button change to resume if already paused
        public void OnClickPauseButton()
        {
            if (emulator.isRunning)
            {
                TogglePause();
            }
        }

        public void OnClickStepButton()
        {
            if (emulator.isRunning)
            {
                EmulatorStep();
            }
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

        public void StopEmulation() {
            emulator.Stop();
            cToken.Cancel();
            Console.WriteLine("Emulation stopped");
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
			emulator.paused = true; //Pause the emulator;
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
		}

		private void OnKeyUp(object? sender, KeyEventArgs e) {
			if(buttonKeys.ContainsKey(e.Key)) {
				Input.OnButtonUp(buttonKeys[e.Key]);
			}
		}

	}
}
