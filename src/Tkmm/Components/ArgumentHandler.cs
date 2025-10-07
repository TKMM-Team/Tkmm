using Avalonia.Controls;
using Avalonia.Threading;
using ConfigFactory.Models;
using Microsoft.Extensions.Logging;
using Tkmm.Actions;
using Tkmm.CLI;
using Tkmm.ViewModels.Pages;
using TkSharp.Core;

namespace Tkmm.Components;

internal static class ArgumentHandler
{
    private static bool _wired;

    public static void EnsureWired()
    {
        if (_wired) {
            return;
        }

        TkConsoleApp.InstallRequested += async (arg, stream) => await ModActions.Instance.Install(arg, stream);

        TkConsoleApp.OpenModRequested += async (modId, fileId) => {
            try {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    PageManager.Shared.Focus(Page.GbMods);
                    var gameBananaPage = PageManager.Shared.Get<GameBananaPageViewModel>(Page.GbMods);
                    await gameBananaPage.OpenModInViewerAsync(modId, fileId);
                });
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, "Error opening mod: {Message}", ex.Message);
            }
        };

        TkConsoleApp.PageRequested += name => {
            try {
                Dispatcher.UIThread.Post(() => {
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
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, "Error navigating to page: {Page}", name);
            }
        };

        TkConsoleApp.SettingsFocusRequested += section => {
            try {
                Dispatcher.UIThread.Post(() => {
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
                });
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, "Error focusing settings section: {Section}", section);
            }
        };

        _wired = true;
    }
}