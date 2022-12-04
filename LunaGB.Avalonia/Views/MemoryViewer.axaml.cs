using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LunaGB.Core;

namespace LunaGB.Avalonia.Views
{
	public partial class MemoryViewer : Window
	{
		public Emulator emulator;
		//TextBox textbox;

		public MemoryViewer()
		{
			DataContext = this;
			AvaloniaXamlLoader.Load(this);
			//textbox = this.FindControl<TextBox>("textbox");

			//textbox.Text = "hello";
			//textbox.IsReadOnly = true;
		}

		protected override bool HandleClosing()
		{
			return base.HandleClosing();
		}
	}
}