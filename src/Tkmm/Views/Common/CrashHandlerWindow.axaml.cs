using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Tkmm.Core.Services;

#if SWITCH
using Tkmm.Models.MenuModels;
#endif

namespace Tkmm.Views.Common;

public partial class CrashHandlerWindow : Window
{
    public CrashHandlerWindow() : this(string.Empty)
    {
    }

    public CrashHandlerWindow(string crashInfoPath)
    {
        InitializeComponent();

#if SWITCH
        DesktopButtons.IsVisible = false;
        SwitchButtons.IsVisible = true;
#endif

        DetailsTextBox.Text = File.Exists(crashInfoPath)
            ? File.ReadAllText(crashInfoPath)
            : "No crash details were provided.";
    }

    private void Copy_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Clipboard is not { } clipboard) {
            return;
        }

        var details = DetailsTextBox.Text ?? string.Empty;
        clipboard.SetTextAsync($"```\n{details}\n```");
    }

    private void Restart_OnClick(object? sender, RoutedEventArgs e)
    {
        var sourcePath = Environment.ProcessPath ?? Environment.GetCommandLineArgs().FirstOrDefault();
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath)) {
            return;
        }

        SingleInstanceAppManager.MarkRestarting();
        CrashHandler.ReleaseForRestart();

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sourcePath) {
            UseShellExecute = true,
            WorkingDirectory = AppContext.BaseDirectory
        });
        CloseApp();
    }

    private void Reboot_OnClick(object? sender, RoutedEventArgs e)
    {
#if SWITCH
        NxMenuModel.Reboot();
#endif
    }

    private void Shutdown_OnClick(object? sender, RoutedEventArgs e)
    {
#if SWITCH
        NxMenuModel.Shutdown();
#endif
    }

    private void Exit_OnClick(object? sender, RoutedEventArgs e)
    {
#if SWITCH
        CrashHandler.StartTkmmService();
#endif
        CloseApp();
    }

    private static void CloseApp()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.Shutdown();
            return;
        }

        Environment.Exit(0);
    }
}