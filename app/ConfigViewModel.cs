using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive;
using ReactiveUI;
using VMGuide;

namespace app
{
    public class ConfigViewModel: ReactiveObject, IRoutableViewModel
    {
        public ObservableList<FormProperty> FormProperties { get; }
            = new ObservableList<FormProperty>();
        
        public IScreen HostScreen { get; }
        public string UrlPathSegment => "ConfigView";

        private IVirtualMachine CurrentVM = null;

        public ReactiveCommand<Unit, Unit> Test { get; }

        public ConfigViewModel(IScreen screen, IVirtualMachine vm) {
            HostScreen = screen;

            CurrentVM = vm;
            CurrentVM.Load();

            FormProperties.AddRange(
                FormProperty.GetFormProperties(CurrentVM)
            );
        }
    }
}