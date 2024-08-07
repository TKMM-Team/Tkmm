﻿using Avalonia;
using Avalonia.Threading;
using Cocona;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System.Diagnostics;
using Tkmm.Components;
using Tkmm.Core.Commands;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers.Win32;
using Tkmm.Helpers;

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

        if (CheckArgs(args) == 0) {
            try {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex) {
                if (OperatingSystem.IsWindows()) {
                    WindowsOperations.SetWindowMode(WindowMode.Visible);
                }

                Trace.WriteLine(ex);
            }

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
        foreach (string arg in args.Where(x => Path.Exists(x) || (x.Length > 5 && x.AsSpan()[..5] is "tkmm:"))) {
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
            .Register(new FontAwesomeIconProvider(FontAwesomeJsonStreamProvider.Instance));

#if DEBUG
        if (OperatingSystem.IsWindows() && Core.Config.Shared.ShowConsole == false) {
            Core.Helpers.Win32.WindowsOperations.SetWindowMode(Core.Helpers.Win32.WindowMode.Hidden);
        }
#endif

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont();
    }
}
