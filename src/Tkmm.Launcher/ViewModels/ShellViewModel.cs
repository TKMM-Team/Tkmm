using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;

namespace Tkmm.Launcher.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string _primaryText = "Install";

    public ShellViewModel()
    {
        Init();
    }

    [RelayCommand]
    private static async Task Update()
    {
        await AppManager.Update();
        await ToolHelper.DownloadDependencies();
    }

    [RelayCommand]
    private static void Exit()
    {
        Environment.Exit(0);
    }

    private async void Init()
    {
        if (await AppManager.HasUpdate()) {
            PrimaryText = "Update";
        }
        else {
            PrimaryText = "Launch";
        }
    }
}
