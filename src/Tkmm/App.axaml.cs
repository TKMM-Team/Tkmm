﻿using System.Reflection;
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
using Tkmm.Core.Logging;
using Tkmm.Extensions;
using Tkmm.ViewModels;
using Tkmm.Views;
using Tkmm.Views.Pages;
using TkSharp.Core;
using TkSharp.Core.Models;

#if SWITCH
using Tkmm.Components.NX;
using Tkmm.Models.MenuModels;
using Tkmm.VirtualKeyboard.Extensions;
#endif

namespace Tkmm;

public class App : Application
{
    private static WindowNotificationManager? _notificationManager;

    public static readonly string Version = typeof(App).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion.Split('+')[0] ?? Locale[TkLocale.UndefinedVersion];

    public static string Title => "TotK Mod Manager";

    public static string ShortTitle { get; } = $"TKMM v{Version}";

    public static IMenuFactory MenuFactory { get; private set; } = null!;

    public static TopLevel XamlRoot { get; private set; } = null!;

    static App()
    {
#if !SWITCH
        ExportLocationControlBuilder.Shared.Register();
#endif
        PathCollectionControlBuilder.Shared.Register();
    }

    public App()
    {
        TkLog.Instance.LogInformation(
            "Version: {Version}", Version);

        TaskScheduler.UnobservedTaskException += (_, eventArgs) => {
            TkLog.Instance.LogError(
                eventArgs.Exception, "Unobserved task exception");
            
            eventArgs.SetObserved();
            TkStatus.SetTemporaryShort(
                $"{eventArgs.Exception.GetType().ToString().Humanize(LetterCasing.Title)} occured",
                TkIcons.ERROR
            );
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
        Config.Shared.GetLanguages = () => Locale.Languages;

        EventLogger.OnLog += (level, eventId, exception, message) => {
            if (level is not LogLevel.Error) {
                return;
            }

            Toast(message, $"{exception?.GetType().Name.Humanize() ?? "Error"} ({eventId})",
                NotificationType.Error,
                action: () => PageManager.Shared.Focus(Page.Logs));
        };

        ShellView shellView = new() {
            DataContext = ShellViewModel.Shared
        };
        
#if SWITCH
        shellView.AddVirtualKeyboard();
        shellView.PowerOptionsMenu.IsVisible = true;
        shellView.NxBatteryStatusPanel.IsVisible = true;
        
        AvaloniaMenuFactory nxSystemMenu = new(XamlRoot,
            localeKeyName => Locale[localeKeyName, failSoftly: true]
        );
        nxSystemMenu.AddMenuGroup<NxMenuModel>();
        shellView.PowerOptionsMenu.ItemsSource = nxSystemMenu.Items;
        
        BatteryStatusWatcher.Start();
#endif

        shellView.InitializeWizard();
        
        shellView.Closed += async (_, _) => { await SystemActions.SoftClose(); };

        XamlRoot = shellView;
        shellView.Loaded += (_, _) => {
            _notificationManager = new WindowNotificationManager(XamlRoot) {
                Position = NotificationPosition.BottomRight,
                MaxItems = 1,
                Margin = new Thickness(0, 0, 4, 30)
            };
        };

        MenuFactory = new AvaloniaMenuFactory(XamlRoot,
            localeKeyName => Locale[localeKeyName, failSoftly: true]
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

            settingsModel.AppendAndValidate<Config>(ref isValid);
            settingsModel.AppendAndValidate<GbConfig>(ref isValid);
            settingsModel.AppendAndValidate<TkConfig>(ref isValid);
        }

        PageManager.Shared.Register(Page.Home, TkLocale.HomePageTitle, new HomePageView(), Symbol.Home, TkLocale.HomePageDesc, isDefault: true);
        PageManager.Shared.Register(Page.Profiles, TkLocale.ProfilesPageTitle, new ProfilesPageView(), Symbol.OtherUser, TkLocale.ProfilesPageDesc);
        PageManager.Shared.Register(Page.Tools, TkLocale.ProjectsPageTitle, new ProjectsPageView(), Symbol.CodeHTML, TkLocale.ProjectsPageDesc);
        PageManager.Shared.Register(Page.GbMods, TkLocale.GameBananaPageTitle, new GameBananaPageView(), Symbol.Globe, TkLocale.GameBananaPageDesc);
        PageManager.Shared.Register(Page.TotKOptimizer, TkLocale.TotkOptimizerPageTitle, new TkOptimizerPageView(), Symbol.StarEmphasis, TkLocale.TotkOptimizerPageDesc, onPageFocused: TkOptimizerPageView.OnPageFocused);
        PageManager.Shared.Register(Page.Cheats, TkLocale.CheatsPageTitle, new TkCheatsPageView(), Symbol.Games, TkLocale.CheatsPageDesc, onPageFocused: TkCheatsPageView.OnPageFocused);

        PageManager.Shared.Register(Page.Logs, TkLocale.LogsPageTitle, new LogsPageView(), Symbol.AllApps, TkLocale.LogsPageDesc, isFooter: true);
#if SWITCH
        PageManager.Shared.Register(Page.NetworkSettings, TkLocale.NetworkSettingsPageTitle, new NetworkSettingsPageView(), Symbol.Wifi4, TkLocale.NetworkSettingsPageDesc, isFooter: true);
#endif
        PageManager.Shared.Register(Page.Settings, TkLocale.SettingsPageTitle, settingsPage, Symbol.Settings, TkLocale.SettingsPageDesc, isFooter: true, isDefault: isValid == false);

        OnThemeChanged(Config.Shared.Theme);

        ArgumentHandler.EnsureWired();
        
        base.OnFrameworkInitializationCompleted();
        
        Dispatcher.UIThread.Post(async () => {
            await Task.Delay(1000);
            Program.ProcessStartupArgs();
        }, DispatcherPriority.Loaded);
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
#if !SWITCH
        if (desktop.MainWindow.WindowState == WindowState.Minimized) {
            desktop.MainWindow.WindowState = WindowState.Normal;
        }
#endif
        desktop.MainWindow.Activate();
    }

    public static void Toast(string message, string? title = null, NotificationType type = NotificationType.Information,
        TimeSpan? expiration = null, Action? action = null)
    {
        Dispatcher.UIThread.Invoke(() => {
            _notificationManager?.Show(
                new Notification(title ??= Locale[TkLocale.NoticePopupMessage], message, type, expiration, action));
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