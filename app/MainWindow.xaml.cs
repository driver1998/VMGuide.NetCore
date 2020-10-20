using Avalonia;
using ReactiveUI;
using System;
using Avalonia.Markup.Xaml;
using Avalonia.Diagnostics;
using Avalonia.ReactiveUI;
using SkiaSharp;

namespace app
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            this.WhenActivated(disposed => {});
            this.AttachDevTools();
            
            AvaloniaXamlLoader.Load(this);
        }
    }
}