using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using ConfigFactory.Avalonia.Helpers;
using FluentAvalonia.UI.Controls;
using System.Reflection;
using Tcml.Builders;
using Tcml.Builders.MenuModels;
using Tcml.Core;
using Tcml.Helpers;
using Tcml.ViewModels;
using Tcml.Views;
using Tcml.Views.Pages;

namespace Tcml;

public partial class App : Application
{
    public static string AppName { get; } = $"Totk Cross-platform Mod Loader - v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}";
    public static string CondensedAppName { get; } = $"TCML v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}";

    /// <summary>
    /// Application <see cref="IMenuFactory"/> (used for extending the main menu at runtime)
    /// </summary>
    public static IMenuFactory MenuFactory { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ShellView shellView = new() {
                DataContext = new ShellViewModel()
            };

            desktop.MainWindow = shellView;

            MenuFactory = new MenuFactory(desktop.MainWindow);
            MenuFactory.Append<ShellViewMenu>(new());

            shellView.MainMenu.ItemsSource = MenuFactory.Items;

            // ConfigFactory Configuration
            BrowserDialog.StorageProvider = desktop.MainWindow.StorageProvider;
            Config.SetTheme = (theme) => {
                RequestedThemeVariant = theme == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;
            };

            PageManager.Shared.Register("Home", new HomePageView(), Symbol.Home, "Home");
            PageManager.Shared.Register("Tools", new ToolsPageView(), Symbol.CodeHTML, "Mod Developer Tools");
            PageManager.Shared.Register("About", new HomePageView(), Symbol.Home, "About The Project");

            shellView.MainNavigation.SelectedItem = PageManager.Shared.Pages[0];
        }

        base.OnFrameworkInitializationCompleted();
    }
}
