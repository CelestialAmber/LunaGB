using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LunaGB.Core;
using System;
using System.Text;

namespace LunaGB.Avalonia.Views
{
	public partial class MemoryViewer : Window
	{
		Emulator emulator;
		byte[] memoryBytes = new byte[0x10000];

		public MemoryViewer(Emulator emulator)
		{
			InitializeComponent();

			this.emulator = emulator;
			UpdateMemoryView();
		}

		void UpdateMemoryArray(){
			Memory mem = emulator.GetMemory();

			for(int i = 0; i < 0x10000; i++){
				memoryBytes[i] = mem.GetByte(i);
			}
		}

		private void OnHitUpdateButton(object sender, RoutedEventArgs e){
			UpdateMemoryView();
		}

		void UpdateMemoryView(){
			UpdateMemoryArray();

			StringBuilder sb = new StringBuilder();

			//Print top byte line
			sb.AppendLine("         00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
			sb.AppendLine();

			for(int i = 0; i < 0x10000; i += 16){
				sb.Append("0x" + i.ToString("X4") + "   ");
				//Print row bytes as numbers
				for(int j = 0; j < 16; j++){
					sb.Append(memoryBytes[i + j].ToString("X2") + " ");
				}

				sb.Append("| ");

				//Print row bytes as ASCII
				for(int j = 0; j < 16; j++){
					char c = (char)memoryBytes[i + j];
					if(Char.IsControl(c)) c = ' '; //Display unprintable characters as spaces
					sb.Append(c);
				}

				sb.AppendLine();
			}

			textbox.Text = sb.ToString();
		}

	}
}