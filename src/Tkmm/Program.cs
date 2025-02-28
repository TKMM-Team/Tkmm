﻿using Avalonia;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Tkmm.Actions;
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

            // Will lock until the old
            // files can be deleted
            AppUpdater.CleanupUpdate();

            const string logCategoryName = nameof(TKMM);
            TkLog.Instance.Register(new DesktopLogger(logCategoryName));
            TkLog.Instance.Register(new EventLogger(logCategoryName));

            // Reroute backend localization interface
            TkLocalizationInterface.GetLocale = (key, failSoftly) => Locale[key, failSoftly];
            TkLocalizationInterface.GetCultureName = culture => Locale["Language", failSoftly: false, culture];

            _ = Task.Run(
                () => TkConsoleApp.ProcessBasicArgs(args, (arg, stream) => ModActions.Instance.Install(arg, stream))
            );

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

    public static void Attach(string[] args)
    {
        if (!TkConsoleApp.IsComplexRequest(args)) {
            Dispatcher.UIThread.Invoke(App.Focus);
            _ = Task.Run(
                () => TkConsoleApp.ProcessBasicArgs(args, (arg, stream) => ModActions.Instance.Install(arg, stream))
            );
            return;
        }

        TkConsoleApp.StartCli(args);
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