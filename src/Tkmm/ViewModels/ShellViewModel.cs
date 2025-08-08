#if SWITCH
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
#endif
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;

namespace Tkmm.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    public static readonly ShellViewModel Shared = new();
    
    [ObservableProperty]
    private bool _isFirstTimeSetup = true;

    [ObservableProperty]
    private string _batteryIcon = string.Empty;

    [ObservableProperty]
    private int _batteryCharge = -1;

    [ObservableProperty]
    private bool _isVisibleR2CMenu;

    [ObservableProperty]
    private double _r2CMenuOpacity;
    
    public ShellViewModel()
    {
        IsFirstTimeSetup = !Config.Shared.ConfigExists() || TKMM.TryGetTkRom() is null;
    }
#if SWITCH
    [RelayCommand]
    public void ShowR2CMenu()
    {
        IsVisibleR2CMenu = true;
        R2CMenuOpacity = 1.0;
    }

    [RelayCommand]
    public void HideR2CMenu()
    {
        R2CMenuOpacity = 0.0;
        // hide after fade animation completes
        var timer = new System.Timers.Timer(300);
        timer.Elapsed += (_, _) => {
            Dispatcher.UIThread.Post(() => {
                IsVisibleR2CMenu = false;
            });
            timer.Dispose();
        };
        timer.Start();
    }
#endif
}