using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using ConfigFactory;
using ConfigFactory.Avalonia;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core;
using ConfigFactory.Core.Models;
using ConfigFactory.Models;
using FluentAvalonia.UI.Controls;
using MenuFactory;
using MenuFactory.Abstractions;
using Tkmm.Builders;
using Tkmm.Builders.MenuModels;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Windows;
using Tkmm.Helpers;
using Tkmm.ViewModels;
using Tkmm.Views;
using Tkmm.Views.Pages;
using TotkCommon;
using WindowsOperations = Tkmm.Core.Helpers.Windows.WindowsOperations;

namespace Tkmm;

public class App : Application
{
    private static WindowNotificationManager? _notificationManager;

    public static readonly string Version = typeof(App).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion.Split('+')[0] ?? "Undefined Version";
    public static string Title { get; } = $"TotK Mod Manager";
    public static string ShortTitle { get; } = $"TKMM v{Version}";
    public static string ReleaseUrl { get; } = $"https://github.com/TKMM-Team/Tkmm/releases/{Version}";
    public static TopLevel? XamlRoot { get; private set; }

    /// <summary>
    /// Application <see cref="IMenuFactory"/> (used for extending the main menu at runtime)
    /// </summary>
    public static IMenuFactory MenuFactory { get; private set; } = null!;

    static App()
    {
        ExportLocationControlBuilder.Shared.Register();
    }

    public App()
    {
        TaskScheduler.UnobservedTaskException += (_, e) => {
            ToastError(e.Exception);
        };
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (OperatingSystem.IsWindows()) {
            WindowsOperations.SetWindowMode(Config.Shared.ShowConsole ? WindowMode.Visible : WindowMode.Hidden);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            ShellView shellView = new() {
                DataContext = new ShellViewModel()
            };

            shellView.Closed += (_, _) => {
                ProfileManager.Shared.Apply();
                Config.Shared.Save();
            };

            XamlRoot = shellView;
            shellView.Loaded += (_, _) => {
                _notificationManager = new(XamlRoot) {
                    Position = NotificationPosition.BottomRight,
                    MaxItems = 5,
                    Margin = new(0, 0, 4, 30)
                };
            };

            MenuFactory = new AvaloniaMenuFactory(XamlRoot);
            MenuFactory.AddMenuGroup<ShellViewMenu>();

            var powerOptionsMenu = new AvaloniaMenuFactory(XamlRoot);
            powerOptionsMenu.AddMenuGroup<PowerOptionsMenu>();

            shellView.MainMenu.ItemsSource = MenuFactory.Items;

            #if SWITCH
            shellView.PowerOptionsMenu.ItemsSource = powerOptionsMenu.Items;
            #endif

            desktop.MainWindow = shellView;

            // ConfigFactory Configuration
            BrowserDialog.StorageProvider = desktop.MainWindow.StorageProvider;
            Config.SetTheme = (theme) => {
                RequestedThemeVariant = theme == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;
            };

            ConfigPage settingsPage = new();
            bool isValid = false;

            if (settingsPage.DataContext is ConfigPageModel settingsModel) {
                settingsModel.SecondaryButtonIsEnabled = false;

                isValid = ConfigModule<Config>.Shared.Validate(out string? _, out ConfigProperty? target);
                settingsModel.Append<Config>();

                isValid = isValid && ConfigModule<TotkConfig>.Shared.Validate(out string? _, out target);
                settingsModel.Append<TotkConfig>();

                if (!isValid && target?.Attribute is not null) {
                    settingsModel.SelectedGroup = settingsModel.Categories
                        .Where(x => x.Header == target.Attribute.Category)
                        .SelectMany(x => x.Groups)
                        .FirstOrDefault(x => x.Header == target.Attribute.Group);

                    AppStatus.Set($"Invalid setting, {target.Property.Name} is invalid.",
                        "fa-solid fa-triangle-exclamation", isWorkingStatus: false);
                }
            }

            PageManager.Shared.Register(Page.Home, "Home", new HomePageView(), Symbol.Home, "Home", isDefault: true);
            PageManager.Shared.Register(Page.Profiles, "Profiles", new ProfilesPageView(), Symbol.OtherUser, "Manage mod profiles");
            PageManager.Shared.Register(Page.Tools, "TKCL Packager", new PackagingPageView(), Symbol.CodeHTML, "Mod developer tools");
            PageManager.Shared.Register(Page.ShopParam, "ShopParam Overflow Editor", new ShopParamPageView(), Symbol.Sort, "ShopParam overflow ordering tools");
            PageManager.Shared.Register(Page.Mods, "GameBanana Mod Browser", new GameBananaPageView(), Symbol.Globe, "GameBanana browser client for TotK mods");

            PageManager.Shared.Register(Page.Logs, "Logs", new LogsPageView(), Symbol.AllApps, "System Logs", isFooter: true);
            PageManager.Shared.Register(Page.Settings, "Settings", settingsPage, Symbol.Settings, "Settings", isFooter: true, isDefault: isValid == false);

            Config.SetTheme(Config.Shared.Theme);
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

    public static void Toast(string message, string title = "Notice", NotificationType type = NotificationType.Information, TimeSpan? expiration = null, Action? action = null)
    {
        Dispatcher.UIThread.Invoke(() => {
            _notificationManager?.Show(
                new Notification(title, message, type, expiration, action));
        });
    }

    public static void ToastError(Exception ex)
    {
        AppLog.Log(ex);

        Dispatcher.UIThread.Invoke(() => {
            _notificationManager?.Show(new Notification(
                ex.GetType().Name, ex.Message, NotificationType.Error, onClick: () => {
                    PageManager.Shared.Focus(Page.Logs);
                }));
        });
    }

    public static void LogTkmmInfo()
    {
        AppLog.Log($"App Version: '{Version}'", LogLevel.Info);

        AppLog.Log($"Configured GamePath: '{TotkConfig.Shared.GamePath}'", LogLevel.Info);
        AppLog.Log($"ZsDic Exists: '{File.Exists(TotkConfig.Shared.ZsDicPath)}'", LogLevel.Info);

        AppLog.Log($"TotkCommon Configured GamePath: '{Totk.Config.GamePath}'", LogLevel.Info);
        AppLog.Log($"TotkCommon ZsDic Exists: '{File.Exists(Totk.Config.ZsDicPath)}'", LogLevel.Info);
    }

    public static async Task PromptUpdate()
    {
        ContentDialog dialog = new() {
            Title = "Update",
            Content = """
                An update is available.
                
                Would you like to close your current session and open the launcher?
                """,
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "Cancel"
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary) {
            await Task.Run(async () => {
                await AppManager.UpdateLauncher();
                AppManager.StartLauncher();
            });

            Environment.Exit(0);
        }
    }
}
