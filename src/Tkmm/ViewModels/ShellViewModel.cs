using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string _batteryIcon = string.Empty;

    [ObservableProperty]
    private string _batteryStatus = string.Empty;

    public void UpdateBatteryStatus(string status, string icon)
    {
        BatteryStatus = status;
        BatteryIcon = icon;
    }
}