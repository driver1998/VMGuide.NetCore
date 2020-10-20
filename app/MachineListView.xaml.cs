using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Dialogs;
using Avalonia.ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Threading;
using VMGuide;
using ReactiveUI;

namespace app
{
    public class MachineListView : ReactiveUserControl<MachineListViewModel>
    {
        public MachineListView()
        {
            this.WhenActivated(disposables => {});
            AvaloniaXamlLoader.Load(this);
            
            this.AttachedToVisualTree += (s, e) => {    
                this.Focus();
            };
        }
    }
}