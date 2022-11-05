using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace LunaGB.Avalonia
{
    internal static class Program {
        [STAThread]
        private static void Main() {
			BuildAvaloniaApp().StartWithClassicDesktopLifetime(null);
        }
        /// <summary>This method is needed for IDE previewer infrastructure.</summary>
        public static AppBuilder BuildAvaloniaApp() {
            return AppBuilder.Configure<App>()
                           .UsePlatformDetect()
                           .UseReactiveUI()
                           .LogToTrace();
        }
    }
}

