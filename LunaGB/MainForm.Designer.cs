using System;
using Modern.Forms;
using SkiaSharp;

namespace LunaGB
{

	public partial class MainForm
	{
		private int windowScale = 3;
		private int width;
		private int height;

		private PictureBox pictureBox;
		private ToolBar toolbar;

		private void InitializeComponent() {
			Text = "LunaGB";

			width = 160 * windowScale;
			height = 144 * windowScale;
			Size = new System.Drawing.Size(width, height);

			
			pictureBox = Controls.Add(new PictureBox());
			pictureBox.Width = width;
			pictureBox.Height = height;
			pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox.Dock = DockStyle.Fill;
			pictureBox.Style.BackgroundColor = new SKColor(0, 0, 0);


			toolbar = Controls.Add(new ToolBar());

			MenuItem fileTab = toolbar.Items.Add(new MenuItem("File"));
			MenuItem emulationTab = toolbar.Items.Add(new MenuItem("Emulation"));
			MenuItem toolsTab = toolbar.Items.Add(new MenuItem("Tools"));
            MenuItem helpTab = toolbar.Items.Add(new MenuItem("Help"));

            //File tab options
            MenuItem openRom = fileTab.Items.Add("Open ROM");
			openRom.Click += (o, e) => OpenRom();

			//Emu tab options
			MenuItem pauseOption = emulationTab.Items.Add("Pause");
			MenuItem stopOption = emulationTab.Items.Add("Stop");

            //Tools tab options
            MenuItem debuggerOption = toolsTab.Items.Add("Debugger");
            debuggerOption.Click += (o, e) => OnClickDebuggerOption();

            //Help tab options
            MenuItem aboutOption = helpTab.Items.Add("About");
			aboutOption.Click += (o, e) => OnClickAbout();
		}
	}
}

