using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.IO;
using Avalonia.Controls;
using ReactiveUI;
using VMGuide;

namespace app
{
    public class MachineListViewModel : ReactiveObject, IRoutableViewModel
    {
        public ObservableList<IGrouping<string, IVirtualMachine>> Machines { get; }
            = new ObservableList<IGrouping<string, IVirtualMachine>>();

        public IScreen HostScreen { get; }
        public string UrlPathSegment => "MachineListView";

        public ReactiveCommand<Unit, Unit> Refresh { get; }
        public ReactiveCommand<Window, Unit> AboutDialog { get; }
        public ReactiveCommand<Window, Unit> OpenMachineDialog { get; }
        public ReactiveCommand<IVirtualMachine, IRoutableViewModel> LoadMachine { get; }

        public MachineListViewModel(IScreen screen)
        {
            HostScreen = screen;

            AboutDialog = ReactiveCommand.CreateFromTask<Window, Unit>(async window =>
            {
                await new AboutWindow().ShowDialog(window);
                return default(Unit);
            });

            LoadMachine = ReactiveCommand.CreateFromObservable<IVirtualMachine, IRoutableViewModel>(
                vm => HostScreen.Router.Navigate.Execute(new ConfigViewModel(HostScreen, vm))
            );

            OpenMachineDialog = ReactiveCommand.CreateFromTask<Window, Unit>(async window =>
            {
                var ext = Core.GetSupportedFileExtensions()
                    .Select(p => p.TrimStart('.')).ToList();

                var filters = new List<FileDialogFilter>();
                filters.Add(new FileDialogFilter()
                {
                    Name = "All supported formats",
                    Extensions = ext
                });

                var files = await new OpenFileDialog()
                {
                    Title = "Open Virtual Machine",
                    Filters = filters,
                    AllowMultiple = false
                }.ShowAsync(window);

                var file = files?.FirstOrDefault();
                if (file != null && File.Exists(file))
                    await LoadMachine.Execute(Core.OpenVirtualMachine(file));

                return default(Unit);
            });

            Refresh = ReactiveCommand.Create(() => {
                Machines.Clear();
                Machines.AddRange(
                    Core.SearchVirtualMachine()
                        .GroupBy(p => p.GetTypeDescription())
                );
            });

            
            Machines.Clear();
            Machines.AddRange(
                Core.SearchVirtualMachine()
                    .GroupBy(p => p.GetTypeDescription())
            );
        }
    }
}