using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Tsukimi.Core;
using Tsukimi.Graphics;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Media;

namespace Tsukimi.Avalonia.Controls
{
	public partial class DisplayView : UserControl
	{

		Image _display;
		TextBlock _fpsText;

		public DisplayView()
		{
			InitializeComponent();
			_display = this.GetControl<Image>("display");
			_fpsText = this.GetControl<TextBlock>("fpsText");
			this.Focusable = true;
			this.PointerPressed += OnClicked;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public void UpdateDisplay(LunaImage image){
			_display.Source = new Bitmap(new MemoryStream(image.ToByteArray()));
		}

		public void UpdateStatusBar(string text){
			_fpsText.Text = text;
		}

		private void OnClicked(object? o, PointerPressedEventArgs e)
		{
			Focus();
		}

	}
}