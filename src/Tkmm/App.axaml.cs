using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using ConfigFactory.Avalonia.Helpers;
using FluentAvalonia.UI.Controls;
using System.Reflection;
using Tkmm.Builders;
using Tkmm.Builders.MenuModels;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
using Tkmm.Helpers;
using Tkmm.ViewModels;
using Tkmm.Views;
using Tkmm.Views.Pages;

namespace Tkmm;

public partial class App : Application
{
    public static string? Version { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
    public static string Title { get; } = $"TotK Mod Manager";
    public static string ShortTitle { get; } = $"TKMM v{Version}";
    public static string ReleaseUrl { get; } = $"https://github.com/TKMM-Team/Tkmm/releases/{Version}";
    public static TopLevel? XamlRoot { get; private set; }

    /// <summary>
    /// Application <see cref="IMenuFactory"/> (used for extending the main menu at runtime)
    /// </summary>
    public static IMenuFactory MenuFactory { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            ShellView shellView = new() {
                DataContext = new ShellViewModel()
            };

            desktop.MainWindow = shellView;
            shellView.Closed += (s, e) => {
                ModManager.Shared.Apply();
                Config.Shared.Save();
            };

            XamlRoot = shellView;

            MenuFactory = new MenuFactory(desktop.MainWindow);
            MenuFactory.Append<ShellViewMenu>(new());

            shellView.MainMenu.ItemsSource = MenuFactory.Items;

            // ConfigFactory Configuration
            BrowserDialog.StorageProvider = desktop.MainWindow.StorageProvider;
            Config.SetTheme = (theme) => {
                RequestedThemeVariant = theme == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;
            };

            PageManager.Shared.Register(Page.Home, "Home", new HomePageView(), Symbol.Home, "Home");
            PageManager.Shared.Register(Page.Tools, "Dev Tools", new PackagingPageView(), Symbol.CodeHTML, "Mod Developer Tools");
            PageManager.Shared.Register(Page.ShopParam, "ShopParam Handler", new ShopParamPageView(), Symbol.Sort, "ShopParam Overflow Order");
            PageManager.Shared.Register(Page.Mods, "Mods", new GameBananaPageView(), Symbol.Globe, "GameBanana Mods");

            PageManager.Shared.Register(Page.About, "About", new AboutPageView(), Symbol.Bookmark, "About The Project", isFooter: true);
            PageManager.Shared.Register(Page.Logs, "Logs", new LogsPageView(), Symbol.AllApps, "System Logs", isFooter: true);

            shellView.MainNavigation.SelectedItem = PageManager.Shared.Pages[0];

            await ToolHelper.LoadDeps();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void Focus()
    {
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null) {
            desktop.MainWindow.WindowState = WindowState.Normal;
            desktop.MainWindow.Activate();
        }
    }
}
