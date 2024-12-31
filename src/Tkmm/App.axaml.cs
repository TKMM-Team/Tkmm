using System.Reflection;
using System.Timers;
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
using ConfigFactory.Models;
using FluentAvalonia.UI.Controls;
using Humanizer;
using MenuFactory;
using MenuFactory.Abstractions;
using Microsoft.Extensions.Logging;
using Tkmm.Actions;
using Tkmm.Builders;
using Tkmm.Components;
using Tkmm.Core;
using Tkmm.Core.Localization;
using Tkmm.Core.Logging;
using Tkmm.Dialogs;
using Tkmm.Extensions;
using Tkmm.Managers;
using Tkmm.ViewModels;
using Tkmm.Views;
using Tkmm.Views.Pages;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm;

public class App : Application
{
    private static WindowNotificationManager? _notificationManager;

    #if SWITCH
    private System.Timers.Timer? _batteryStatusTimer;
    #endif

    public static readonly string Version = typeof(App).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion.Split('+')[0] ?? SystemMsg.UndefinedVersion;

    public static string Title { get; } = "TotK Mod Manager";

    public static string ShortTitle { get; } = $"TKMM v{Version}";

    public static IMenuFactory MenuFactory { get; private set; } = null!;

    public static TopLevel XamlRoot { get; private set; } = null!;

    static App()
    {
        ExportLocationControlBuilder.Shared.Register();
    }

    public App()
    {
        TkLog.Instance.LogInformation(
            "Version: {Version}", Version);

        TaskScheduler.UnobservedTaskException += async (_, eventArgs) => {
            TkLog.Instance.LogError(
                eventArgs.Exception, "Unobserved task exception");

            await ErrorDialog.ShowAsync(eventArgs.Exception, TaskDialogStandardResult.OK);
            eventArgs.SetObserved();
            TkStatus.Reset();
        };
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) {
            return;
        }
        
        BindingPlugins.DataValidators.RemoveAt(0);
        TkThumbnail.CreateBitmap = stream => new Bitmap(stream);

        EventLogger.OnLog += (level, eventId, exception, message) => {
            if (level is not LogLevel.Error) {
                return;
            }
            
            Toast(message, $"{exception?.GetType().Name.Humanize() ?? "Error"} ({eventId})", NotificationType.Error, action: () => {
                PageManager.Shared.Focus(Page.Logs);
            });
        };

        ShellView shellView = new() {
            DataContext = ShellViewModel.Shared
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

        shellView.MainMenu.ItemsSource = MenuFactory.Items;#if SWITCH
            var powerOptionsMenu = new AvaloniaMenuFactory(XamlRoot);
            powerOptionsMenu.AddMenuGroup<PowerOptionsMenu>();
            shellView.PowerOptionsMenu.ItemsSource = powerOptionsMenu.Items;

            if (XamlRoot is ShellView currentShellView)
            {
                var batteryStatusTextBlock = currentShellView.FindControl<TextBlock>("BatteryStatusTextBlock");
                var viewModel = currentShellView.DataContext as ShellViewModel;

                if (viewModel != null)
                {
                    var batteryStatusManager = new BatteryStatusManager(viewModel);

                    // Timer to update battery status every second. Could have been better implemented with something that reads the files dynamically but this is a quick fix
                    _batteryStatusTimer = new System.Timers.Timer(1000);
                    _batteryStatusTimer.Elapsed += (sender, e) =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (batteryStatusTextBlock != null)
                            {
                                batteryStatusManager.UpdateBatteryStatus(batteryStatusTextBlock);
                            }
                            else
                            {
                                throw new InvalidOperationException("BatteryStatusTextBlock cannot be null.");
                            }
                        });
                    };
                    _batteryStatusTimer.AutoReset = true;
                    _batteryStatusTimer.Start();
                }
                else
                {
                    throw new InvalidOperationException("ViewModel cannot be null.");
                }
            }
            #endif
        desktop.MainWindow = shellView;

        // ConfigFactory Configuration
        BrowserDialog.StorageProvider = shellView.StorageProvider;

        Config.Shared.ThemeChanged += OnThemeChanged;

        ConfigPage settingsPage = new();
        bool isValid = false;

        if (settingsPage.DataContext is ConfigPageModel settingsModel) {
            settingsModel.SecondaryButtonIsEnabled = false;
            
            settingsModel.AppendAndValidate<Config>(ref isValid);
            settingsModel.AppendAndValidate<TkConfig>(ref isValid);
        }

        PageManager.Shared.Register(Page.Home, PageMsg.Home, new HomePageView(), Symbol.Home, PageMsg.HomeDescription, isDefault: true);
        PageManager.Shared.Register(Page.Profiles, PageMsg.Profiles, new ProfilesPageView(), Symbol.OtherUser, PageMsg.ProfilesDescription);
        PageManager.Shared.Register(Page.Tools, PageMsg.Tools, new ProjectsPageView(), Symbol.CodeHTML, PageMsg.ToolsDescription);
        PageManager.Shared.Register(Page.GbMods, PageMsg.GbMods, new GameBananaPageView(), Symbol.Globe, PageMsg.GbModsDescription);

        PageManager.Shared.Register(Page.Logs, PageMsg.Logs, new LogsPageView(), Symbol.AllApps, PageMsg.LogsDescription, isFooter: true);
        PageManager.Shared.Register(Page.NetworkSettings, "Network Settings", new NetworkSettingsPageView(), Symbol.Wifi4, "Settings for WiFi and other network services");
        PageManager.Shared.Register(Page.Settings, PageMsg.Settings, settingsPage, Symbol.Settings, PageMsg.SettingsDescription, isFooter: true, isDefault: isValid == false);

        OnThemeChanged(Config.Shared.Theme);

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

    public static void Toast(string message, string? title = null, NotificationType type = NotificationType.Information,
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
                ex.GetType().Name, ex.Message, NotificationType.Error, onClick: () => {
                    PageManager.Shared.Focus(Page.Logs);
                }));
        });
    }
}