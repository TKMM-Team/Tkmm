using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using R2CSharp.Lib.Extensions;
using Tkmm.Actions;
using Tkmm.Components;
using Tkmm.CLI;
using Tkmm.Core;
using Tkmm.Core.Logging;
using Tkmm.Core.Services;
using Tkmm.ViewModels.Pages;
using TkSharp.Core;
using TkSharp.Core.Common;
using ConfigFactory.Models;
using TkSharp.Extensions.GameBanana;

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

            // Will lock until the old files can be deleted
            AppUpdater.CleanupUpdate();

            const string logCategoryName = nameof(TKMM);
            TkLog.Instance.Register(new DesktopLogger(logCategoryName));
            TkLog.Instance.Register(new EventLogger(logCategoryName));

            // Reroute backend localization interface
            TkLocalizationInterface.GetLocale = (key, failSoftly) => Locale[key, failSoftly];
            TkLocalizationInterface.GetCultureName = culture => Locale["Language", failSoftly: false, culture];

            HandleArgs(args);

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

    private static bool _eventsWired;
    private static void HandleArgs(string[] args)
    {
        if (!_eventsWired) {
            TkConsoleApp.InstallRequested += async (arg, stream) => await ModActions.Instance.Install(arg, stream);
            TkConsoleApp.OpenModRequested += async (modId, fileId) => await Dispatcher.UIThread.InvokeAsync(async () => {
                try {
                    PageManager.Shared.Focus(Page.GbMods);
                    var gameBananaPage = PageManager.Shared.Get<GameBananaPageViewModel>(Page.GbMods);
                    await gameBananaPage.OpenModInViewerAsync(modId, fileId);
                }
                catch (Exception ex) {
                    TkLog.Instance.LogError(ex, "Error opening mod: {Message}", ex.Message);
                }
            });
            TkConsoleApp.PageRequested += name => Dispatcher.UIThread.Post(() => {
                var page = name switch {
                    "home" => Page.Home,
                    "profiles" => Page.Profiles,
                    "projects" => Page.Tools,
                    "gamebanana" => Page.GbMods,
                    "optimizer" => Page.TotKOptimizer,
                    "cheats" => Page.Cheats,
                    "logs" => Page.Logs,
                    "settings" => Page.Settings,
                    _ => PageManager.Shared.Current?.Id ?? Page.Home
                };
                PageManager.Shared.Focus(page);
            });
            TkConsoleApp.SettingsFocusRequested += section => Dispatcher.UIThread.Post(() => {
                try {
                    var content = PageManager.Shared[Page.Settings].Content;
                    if (content is UserControl { DataContext: ConfigPageModel m }) {
                        var normalizedHeader = section switch {
                            "application" => "Application",
                            "packaging" => "Packaging",
                            "merging" => "Merging",
                            "gamebanana" or "gamebanana-client" or "gamebanana_client" => "GameBanana Client",
                            "dump" or "game-dump" or "game_dump" => "Game Dump",
                            _ => section
                        };

                        var group = m.Categories
                            .SelectMany(c => c.Groups)
                            .FirstOrDefault(g => string.Equals(g.Header, normalizedHeader, StringComparison.OrdinalIgnoreCase));

                        if (group is not null) {
                            m.SelectedGroup = group;
                        }
                    }
                    PageManager.Shared.Focus(Page.Settings);
                }
                catch (Exception ex) {
                    TkLog.Instance.LogError(ex, "Error focusing settings section: {Section}", section);
                }
            });
            _eventsWired = true;
        }

        TkConsoleApp.ProcessBasicArgs(args);
    }
}