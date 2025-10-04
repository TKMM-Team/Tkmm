using Avalonia;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using R2CSharp.Lib.Extensions;
using Tkmm.Actions;
using Tkmm.Components;
#if !SWITCH
using Tkmm.CLI;
#endif
using Tkmm.Core;
using Tkmm.Core.Logging;
using Tkmm.Core.Services;
using Tkmm.ViewModels.Pages;
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

#if !SWITCH
            if (TkConsoleApp.IsComplexRequest(args)) {
                TkConsoleApp.StartCli(args);
                return;
            }
#endif

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
            
            _ = Task.Run(async () => {
                while (true)
                {
                    if (TkConsoleApp.TryGetPendingModViewerRequest(out var modId))
                    {
                        Console.WriteLine($"Processing mod viewer request for mod ID: {modId}");
                        
                        await Task.Delay(3000);
                        
                        await Dispatcher.UIThread.InvokeAsync(async () => {
                            try
                            {
                                Console.WriteLine("Navigating to GameBanana page...");
                                PageManager.Shared.Focus(Page.GbMods);
                                
                                Console.WriteLine("Getting GameBanana page view model...");
                                var gameBananaPage = PageManager.Shared.Get<GameBananaPageViewModel>(Page.GbMods);
                                
                                Console.WriteLine($"Opening mod {modId} in viewer...");
                                await gameBananaPage.OpenModInViewerAsync(modId);
                                Console.WriteLine("Mod opened successfully!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error opening mod: {ex.Message}");
                            }
                        });
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
            });

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
#if !SWITCH
        if (!TkConsoleApp.IsComplexRequest(args)) {
            Dispatcher.UIThread.Invoke(App.Focus);
            _ = Task.Run(
                () => TkConsoleApp.ProcessBasicArgs(args, (arg, stream) => ModActions.Instance.Install(arg, stream))
            );
            return;
        }

        TkConsoleApp.StartCli(args);
#endif
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
            .UseR2CSharp()
            .UsePlatformDetect()
            .WithInterFont();
    }
}