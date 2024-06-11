using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
using Tkmm.Launcher.Views;

namespace Tkmm.Launcher.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private const string INSTALL = "Install";
    private const string UPDATE = "Update";
    private const string LAUNCH = "Launch";

    private readonly ShellView _view;

    [ObservableProperty]
    private string _primaryText = INSTALL;

    [ObservableProperty]
    private string _status = "Ready";

    [ObservableProperty]
    private double _progress = 0.0;

    [ObservableProperty]
    private bool _showStatusBar = true;

    [ObservableProperty]
    private bool _isInstalled = false;

    [ObservableProperty]
    private bool _installShortcuts = true;

    public ShellViewModel(ShellView view)
    {
        _view = view;
        Init();
    }

    [RelayCommand]
    private async Task Primary()
    {
        if (PrimaryText is INSTALL or UPDATE) {
            if (OperatingSystem.IsWindows()) {
                _view.PlatformFeatures.SetTaskBarProgressBarState(TaskBarProgressBarState.Indeterminate);

            }

            await Task.Run(async () => {
                Progress = 10;
                await AppManager.Update((progress) => Progress = progress);
                await AssetHelper.Download();
                Progress = 98;
            });

            if (InstallShortcuts) {
                AppManager.CreateDesktopShortcuts();
            }

            Progress = 100;

            if (OperatingSystem.IsWindows()) {
                _view.PlatformFeatures.SetTaskBarProgressBarState(TaskBarProgressBarState.Normal);
                _view.PlatformFeatures.SetTaskBarProgressBarValue(0, 0);
            }

            PrimaryText = LAUNCH;
        }
        else {
            AppManager.Start();
        }
    }

    [RelayCommand]
    private static async Task Uninstall()
    {
        await Task.Run(AppManager.Uninstall);
    }

    [RelayCommand]
    private static void Exit()
    {
        Environment.Exit(0);
    }

    private async void Init()
    {
        if (!AppManager.IsInstalled()) {
            PrimaryText = INSTALL;
        }
        else if (await AppManager.HasUpdate()) {
            PrimaryText = UPDATE;
            IsInstalled = true;
        }
        else {
            PrimaryText = LAUNCH;
            ShowStatusBar = false;
            IsInstalled = true;
        }
    }
}
