using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;

namespace Tkmm.Launcher.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private const string INSTALL = "Install";
    private const string UPDATE = "Update";
    private const string LAUNCH = "Launch";

    [ObservableProperty]
    private string _primaryText = INSTALL;

    [ObservableProperty]
    private string _status = "Ready";

    [ObservableProperty]
    private double _progress = 0.0;

    [ObservableProperty]
    private bool _showStatusBar = true;

    public ShellViewModel()
    {
        Init();
    }

    [RelayCommand]
    private async Task Primary()
    {
        if (PrimaryText is INSTALL or UPDATE) {
            await Task.Run(async () => {
                Progress = 10;
                await AppManager.Update();
                Progress = 20;
                await ToolHelper.DownloadDependencies(UpdateProgress);
                Progress = 100;
            });

            PrimaryText = LAUNCH;
        }
        else {
            AppManager.Start();
        }
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
        }
        else {
            PrimaryText = LAUNCH;
            ShowStatusBar = false;
        }
    }

    private void UpdateProgress(double value)
    {
        Progress += value;
    }
}
