using System.Reactive;
using ReactiveUI;
using VMGuide;

namespace app
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        public RoutingState Router { get; }
        public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;
        public MainWindowViewModel()
        {
            Router = new RoutingState();
            Router.Navigate.Execute(new MachineListViewModel(this));
        }
    }
}