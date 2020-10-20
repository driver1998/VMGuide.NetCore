using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Media;
using ReactiveUI;
using Splat;

namespace app
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp() {
            Locator.CurrentMutable.Register(() => new MachineListView(), typeof(IViewFor<MachineListViewModel>));
            Locator.CurrentMutable.Register(() => new ConfigView(), typeof(IViewFor<ConfigViewModel>));
            
            // Try to workaround Linux font loading bug by providing a custom FontManagerImpl
            AvaloniaLocator.CurrentMutable.Bind<FontManager>().ToConstant(new FontManager(new CustomFontManagerImpl()));

            return AppBuilder.Configure<App>()
                .UseReactiveUI()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions { OverlayPopups = true })
                .LogToDebug();
        }
    }
}
