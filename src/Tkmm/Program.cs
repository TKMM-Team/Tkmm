using Avalonia;
using Microsoft.Extensions.Logging;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Tkmm.Components;
using Tkmm.Core;
using TkSharp.Core;

namespace Tkmm;

internal abstract class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An unhandled exception of type '{ErrorType}' occured.", ex.GetType());
#if DEBUG
            throw;
#endif
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register(new FontAwesomeIconProvider(FontAwesomeJsonStreamProvider.Instance));

#if DEBUG
        // if (OperatingSystem.IsWindows() && Config.Shared.ShowConsole == false) {
        //     Core.Helpers.Windows.WindowsOperations.SetWindowMode(Core.Helpers.Windows.WindowMode.Hidden);
        // }
#endif

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont();
    }
}