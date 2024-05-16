using Avalonia;
using Avalonia.Threading;
using Cocona;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Tkmm.Core.Commands;
using Tkmm.Core.Components;
using Tkmm.Helpers;
using Avalonia.Dialogs;

namespace Tkmm.Switch;

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

        if (CheckArgs(args) == 0) {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return;
        }

        CoconaApp app = CoconaApp.Create();
        app.AddCommands<GeneralCommands>();
        app.Run();
    }

    public static async Task Attach(string[] args)
    {
        if (CheckArgs(args) == 0) {
            Dispatcher.UIThread.Invoke(App.Focus);
            return;
        }

        CoconaApp app = CoconaApp.Create(args);
        app.AddCommands<GeneralCommands>();
        await app.RunAsync();
    }

    private static int CheckArgs(string[] args)
    {
        int argc = args.Length;
        foreach (string arg in args.Where(Path.Exists)) {
            argc--;
            _ = Task.Run(async () => {
                await ModHelper.Import(arg);
            });
        }

        return argc;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseManagedSystemDialogs()
            .WithInterFont()
            .LogToTrace();
    }
}
