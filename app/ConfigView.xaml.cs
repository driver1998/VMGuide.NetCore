using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using ReactiveUI;
using Avalonia.ReactiveUI;
using VMGuide;
using System;
using Avalonia.VisualTree;

namespace app
{
    public class ConfigView : ReactiveUserControl<ConfigViewModel>
    {
        public ConfigView()
        {
            this.WhenActivated(disposables => {});
            AvaloniaXamlLoader.Load(this);

            this.AttachedToVisualTree += (s, e) => {    
                this.Focus();
            };
        }
    }
}