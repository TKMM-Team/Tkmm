using Avalonia.Controls;
using Tkmm.ViewModels;

namespace Tkmm.Managers;

public class BatteryStatusManager
{
    private readonly ShellViewModel _viewModel;

    public BatteryStatusManager(ShellViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void UpdateBatteryStatus(TextBlock batteryStatusTextBlock)
    {
        string batteryStatus = "Unknown";
        int batteryCapacity = 0;

        batteryStatus = File.ReadAllText("/sys/class/power_supply/battery/status").Trim();
        batteryCapacity = int.Parse(File.ReadAllText("/sys/class/power_supply/battery/capacity").Trim());

        string batteryIconValue = batteryStatus == "Charging" ? "fa-solid fa-battery-bolt" :
            GetDischargingIcon(batteryCapacity) ;

        _viewModel.UpdateBatteryStatus($"{batteryCapacity}%", batteryIconValue);
    }

    private string GetDischargingIcon(int capacity)
    {
        if (capacity > 87) return "fa-solid fa-battery-full";
        if (capacity > 65) return "fa-solid fa-battery-three-quarters";
        if (capacity > 40) return "fa-solid fa-battery-half";
        if (capacity > 20) return "fa-solid fa-battery-quarter";
        if (capacity > 7) return "fa-solid fa-battery-low";
        return "fa-solid fa-battery-empty";
    }
}