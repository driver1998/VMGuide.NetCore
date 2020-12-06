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