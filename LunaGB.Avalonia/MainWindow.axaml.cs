using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LunaGB.Core;

namespace LunaGB.Avalonia {
    public partial class MainWindow : Window {

        private bool isDisposed;
        private readonly object _updateLock = new object();

        private Dictionary<Key, Button> _controls;
        private byte[] _lastFrame;
        private CancellationTokenSource _cancellation;

        public Emulator emulator;



        public MainWindow() {
            Opened += OnWindowOpenedBindWindowEvents;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            BuildMenuViewModel();
            //BindKeysToButtons();
           // AdjustEmulatorScreenSize();

            _cancellation = new CancellationTokenSource();

            // ConnectEmulatorToUI();

            emulator = new Emulator();

            KeyDown += MainWindow_KeyDown;
        }

        private void OnWindowOpenedBindWindowEvents(object sender, EventArgs e) {
            PropertyChanged += OnWindowSizeChanged;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void BuildMenuViewModel() {
            var vm = new MainWindowViewModel();

            vm.MenuItems = new[]
                {
                new MenuItemViewModel()
                {
                    Header = "_File",
                    Items = new[]
                    {
                        new MenuItemViewModel()
                        {
                            Header = "_Load ROM"
                        }
                    }
                },
                new MenuItemViewModel() {
                    Header = "_Emulator",
                    Items = new MenuItemViewModel[] {
                        new MenuItemViewModel()
                        {
                            Header = "_Pause"
                        },
                        new MenuItemViewModel()
                        {
                            Header = "_Quit"
                        }
                    }
                }
            };

            DataContext = vm;
        }

        /*_controls = new Dictionary<Key, Button>{
                {Key.Left, Button.Left},
                {Key.Right, Button.Right},
                {Key.Up, Button.Up},
                {Key.Down, Button.Down},
                {Key.Z, Button.A},
                {Key.X, Button.B},
                {Key.Enter, Button.Start},
                {Key.Back, Button.Select}
         };*/

/*private void AdjustEmulatorScreenSize() {
    var imageBox = this.FindControl<Image>("ImageBox");
    if (imageBox != null) {
        imageBox.Width = BitmapDisplay.DisplayWidth * 5;
        imageBox.Height = BitmapDisplay.DisplayHeight * 5;

        MinHeight = imageBox.Height + 25;
        MinWidth = imageBox.Width;

        Height = imageBox.Height + 25;
        Width = imageBox.Width;
    }
}

private void ConnectEmulatorToUI() {
    _emulator.Controller = this;
    _emulator.Display.OnFrameProduced += UpdateDisplay;

    KeyDown += EmulatorSurface_KeyDown;
    KeyUp += EmulatorSurface_KeyUp;
    Closed += (_, e) => { _cancellation.Cancel(); };
}*/


private async Task LoadROM() {
    OpenFileDialog dialog = new OpenFileDialog();
    dialog.Title = "Open ROM file";
    FileDialogFilter gbFilter = new FileDialogFilter();
    gbFilter.Name = "GB/GBC";
    gbFilter.Extensions = new List<string>(new string[] { "gb", "gbc" });

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

private void MainWindow_KeyDown(object? sender, KeyEventArgs e) {
    Console.WriteLine("pressed key");
}

}
}