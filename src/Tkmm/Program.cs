using Avalonia;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using R2CSharp.Lib.Extensions;
using Tkmm.Components;
using Tkmm.CLI;
using Tkmm.Core;
using Tkmm.Core.Logging;
using Tkmm.Core.Services;
using TkSharp.Core;
using TkSharp.Core.Common;

namespace Tkmm;

internal abstract class Program
{
    private static string[]? _startupArgs;

    [STAThread]
    public static void Main(string[] args)
    {
        try {
            if (SingleInstanceAppManager.Start(args, Attach) == false) {
                return;
            }

            if (TkConsoleApp.IsComplexRequest(args)) {
                TkConsoleApp.StartCli(args);
                return;
            }

            // Will lock until the old files can be deleted
            AppUpdater.CleanupUpdate();

            const string logCategoryName = nameof(TKMM);
            TkLog.Instance.Register(new DesktopLogger(logCategoryName));
            TkLog.Instance.Register(new EventLogger(logCategoryName));

            // Reroute backend localization interface
            TkLocalizationInterface.GetLocale = (key, failSoftly) => Locale[key, failSoftly];
            TkLocalizationInterface.GetCultureName = culture => Locale["Language", failSoftly: false, culture];

            _startupArgs = args;

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

    private static void Attach(string[] args)
    {
        if (!TkConsoleApp.IsComplexRequest(args)) {
            Dispatcher.UIThread.Invoke(App.Focus);
            HandleArgs(args);
            return;
        }

        TkConsoleApp.StartCli(args);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register(new FontAwesomeIconProvider(FontAwesomeJsonStreamProvider.Instance));

#if DEBUG
        // if (OperatingSystem.IsWindows() && Config.Shared.ShowConsole == false) {
        //     Core.Helpers.Windows.WindowsOperations.SetWindowMode(Core.Helpers.Windows.WindowMode.Hidden);
        // }
#endif

        return AppBuilder.Configure<App>()
            .UseR2CSharp()
            .UsePlatformDetect()
            .WithInterFont();
    }

    private static void HandleArgs(string[] args)
    {
        ArgumentHandler.EnsureWired();
        TkConsoleApp.ProcessBasicArgs(args);
    }

    public static void ProcessStartupArgs()
    {
        if (_startupArgs != null) {
            HandleArgs(_startupArgs);
            _startupArgs = null;
        }
    }
}