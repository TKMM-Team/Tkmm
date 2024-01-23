using Avalonia;
using Avalonia.Threading;
using Cocona;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Tkmm.Core;
using Tkmm.Core.Commands;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers.Win32;

namespace Tkmm.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (AppManager.Start(args, Attach) == false) {
            return;
        }

        if (args.Length == 0) {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return;
        }

        CoconaApp app = CoconaApp.Create();
        app.AddCommands<GeneralCommands>();
        app.Run();
    }

    public static async Task Attach(string[] args)
    {
        if (args.Length == 0) {
            Dispatcher.UIThread.Invoke(App.Focus);
            return;
        }

        CoconaApp app = CoconaApp.Create(args);
        app.AddCommands<GeneralCommands>();
        await app.RunAsync();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>();

#if DEBUG
        if (OperatingSystem.IsWindows() && Config.Shared.ShowConsole == false) {
            WindowsOperations.SetWindowMode(WindowMode.Hidden);
        }
#endif

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont();
    }
}
