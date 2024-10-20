using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
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
using Microsoft.Extensions.Logging;
using Tkmm.Abstractions;
using Tkmm.Actions;
using Tkmm.Builders;
using Tkmm.Components;
using Tkmm.Core;
using Tkmm.Core.Localization;
using Tkmm.Extensions;
using Tkmm.Dialogs;
using Tkmm.ViewModels;
using Tkmm.Views;
using Tkmm.Views.Pages;
using PageManager = Tkmm.Components.PageManager;

namespace Tkmm;

public class App : Application
{
    private static WindowNotificationManager? _notificationManager;

    public static readonly string Version = typeof(App).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion.Split('+')[0] ?? SystemMsg.UndefinedVersion;

    public static string Title { get; } = $"TotK Mod Manager";

    public static string ShortTitle { get; } = $"TKMM v{Version}";

    public static IMenuFactory MenuFactory { get; private set; } = null!;

    public static TopLevel XamlRoot { get; private set; } = null!;

    static App()
    {
        ExportLocationControlBuilder.Shared.Register();
    }

    public App()
    {
        TKMM.Logger.LogInformation(
            "Version: {Version}", Version);

        TaskScheduler.UnobservedTaskException += async (_, eventArgs) => {
            TKMM.Logger.LogError(
                eventArgs.Exception, "Unobserved task exception");

            await ErrorDialog.ShowAsync(eventArgs.Exception, TaskDialogStandardResult.OK);
            eventArgs.SetObserved();
        };
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        BindingPlugins.DataValidators.RemoveAt(0);
        ITkThumbnail.CreateBitmap = stream => new Bitmap(stream);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            ShellView shellView = new() {
                DataContext = new ShellViewModel()
            };

            shellView.Closed += async (_, _) => {
                await SystemActions.Instance.SoftClose();
            };

            XamlRoot = shellView;
            shellView.Loaded += (_, _) => {
                _notificationManager = new WindowNotificationManager(XamlRoot) {
                    Position = NotificationPosition.BottomRight,
                    MaxItems = 5,
                    Margin = new Thickness(0, 0, 4, 30)
                };
            };

            MenuFactory = new AvaloniaMenuFactory(XamlRoot,
                localeKeyName => GetStringResource(StringResources_Menu.GROUP, localeKeyName)
            );
            MenuFactory.ConfigureMenu();

            shellView.MainMenu.ItemsSource = MenuFactory.Items;
            desktop.MainWindow = shellView;

            // ConfigFactory Configuration
            BrowserDialog.StorageProvider = shellView.StorageProvider;

            Config.Shared.ThemeChanged += OnThemeChanged;

            ConfigPage settingsPage = new();
            bool isValid = false;

            if (settingsPage.DataContext is ConfigPageModel settingsModel) {
                settingsModel.SecondaryButtonIsEnabled = false;

                isValid = ConfigModule<Config>.Shared.Validate(out string? _, out ConfigProperty? target);
                settingsModel.Append<Config>();

                if (!isValid && target?.Attribute is not null) {
                    settingsModel.SelectedGroup = settingsModel.Categories
                        .Where(x => x.Header == target.Attribute.Category)
                        .SelectMany(x => x.Groups)
                        .FirstOrDefault(x => x.Header == target.Attribute.Group);

                    TkStatus.Set(Exceptions.InvalidSettings(target.Property.Name), TkIcons.WARNING);
                }
            }

            PageManager.Shared.Register(Page.Home, PageMsg.Home, new HomePageView(), Symbol.Home, PageMsg.HomeDescription, isDefault: true);
            PageManager.Shared.Register(Page.Profiles, PageMsg.Profiles, new ProfilesPageView(), Symbol.OtherUser, PageMsg.ProfilesDescription);
            PageManager.Shared.Register(Page.Tools, PageMsg.Tools, new ProjectsPageView(), Symbol.CodeHTML, PageMsg.ToolsDescription);
            PageManager.Shared.Register(Page.ShopParam, PageMsg.ShopParam, new ShopParamPageView(), Symbol.Sort, PageMsg.ShopParamDescription);
            PageManager.Shared.Register(Page.GbMods, PageMsg.GbMods, new GameBananaPageView(), Symbol.Globe, PageMsg.GbModsDescription);

            PageManager.Shared.Register(Page.Logs, PageMsg.Logs, new LogsPageView(), Symbol.AllApps, PageMsg.LogsDescription, isFooter: true);
            PageManager.Shared.Register(Page.Settings, PageMsg.Settings, settingsPage, Symbol.Settings, PageMsg.SettingsDescription, isFooter: true, isDefault: isValid == false);

            OnThemeChanged(Config.Shared.Theme);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnThemeChanged(string theme)
    {
        RequestedThemeVariant = theme switch {
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Light
        };
    }

    public static void Focus()
    {
        if (Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null) {
            return;
        }

        desktop.MainWindow.WindowState = WindowState.Normal;
        desktop.MainWindow.Activate();
    }

    public static void Toast(string message, string? title = default, NotificationType type = NotificationType.Information,
        TimeSpan? expiration = null, Action? action = null)
    {
        Dispatcher.UIThread.Invoke(() => {
            _notificationManager?.Show(
                new Notification(title ??= SystemMsg.Notice, message, type, expiration, action));
        });
    }

    public static void ToastError(Exception ex)
    {
        Dispatcher.UIThread.Invoke(() => {
            _notificationManager?.Show(new Notification(
                ex.GetType().Name, ex.Message, NotificationType.Error, onClick: () => { PageManager.Shared.Focus(Page.Logs); }));
        });
    }
}