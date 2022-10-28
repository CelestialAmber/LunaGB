using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using LunaGB.Core;
using SixLabors.ImageSharp.PixelFormats;

namespace LunaGB.Avalonia {
    public partial class MainWindow : Window {

        Emulator emulator;

        Image imageBox;


        public MainWindow() {
            DataContext = this;
            AvaloniaXamlLoader.Load(this);

            imageBox = this.FindControl<Image>("ImageBox");

            emulator = new Emulator();

            KeyDown += OnKeyDown;


            SixLabors.ImageSharp.Image<Rgba32> bitmap = new SixLabors.ImageSharp.Image<Rgba32>(160, 144);

            for (int x = 0; x < 160; x++) {
                for (int y = 0; y < 144; y++) {
                    if ((x + y) % 2 == 0) {
                        bitmap[x, y] =  new Rgba32((byte)x, (byte)y, 255);
                    }
                }
            }

           // imageBox.Source = bitmap.ToAvaloniaBitmap();
        }

        protected override bool HandleClosing() {
            return base.HandleClosing();
        }

        public static void Nyeh() {
            Console.WriteLine("Maaaagic...");
        }


        public async void LoadROM() {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filters = new List<FileDialogFilter>();
            dialog.Title = "Open ROM file";
            FileDialogFilter gbFilter = new FileDialogFilter();
            gbFilter.Name = "GB/GBC";
            gbFilter.Extensions = new List<string>(new string[] { "gb", "gbc" });
            dialog.Filters.Add(gbFilter);

            dialog.Filters.Add(new FileDialogFilter() {
                Name = "All files(*.*)",
                Extensions = { "*.*" }
            });

             var result = await dialog.ShowAsync(this);


            if (result != null) {
                 string filename = result[0];
                 if (filename != null) {
                     string romName = filename;
                     Console.WriteLine("Loading \"" + romName + "\"");
                     emulator.LoadROM(romName);
                 }
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

        private void OnKeyDown(object? sender, KeyEventArgs e) {
            Console.WriteLine("pressed key");
        }

    }
}
