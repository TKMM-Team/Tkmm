using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
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

    public ShellViewModel()
    {
        Init();
    }

    [RelayCommand]
    private async Task Primary()
    {
        if (PrimaryText is INSTALL or UPDATE) {
            await AppManager.Update();
            await ToolHelper.DownloadDependencies();
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
        }
    }
}
