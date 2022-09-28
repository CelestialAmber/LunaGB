using System;
using Modern.Forms;
using SkiaSharp;

namespace LunaGB
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();

			SKBitmap bitmap = new SKBitmap(160, 144, true);

			for(int x = 0; x < 160; x++) {
				for (int y = 0; y < 144; y++) {
					if ((x + y) % 2 == 0) {
						bitmap.SetPixel(x, y, new SKColor((byte)x, (byte)y, 255));
					}
				}
			}

			pictureBox.Image = bitmap;
		}

		async void OpenRom() {
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Open ROM file";
			dialog.AddFilter("GB/GBC", new string[]{"gb", "gbc"});

			if((await dialog.ShowDialog(this)) == DialogResult.OK) {
				if (dialog.FileName != null) {
					string romName = dialog.FileName;
					Console.WriteLine("Loading \"" + romName + "\"");
				}
            }
        }

		void OnClickDebuggerOption() {
			//open debugger window
        }

		void OnClickAbout() {
			//open about window
        }
	}
}

