using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.ViewModels
{
    public partial class ShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _batteryIcon = string.Empty; // Initialize with a default value

        [ObservableProperty]
        private string _batteryStatus = string.Empty; // Initialize with a default value

        public void UpdateBatteryStatus(string status, string icon)
        {
            BatteryStatus = status;
            BatteryIcon = icon;
        }
    }
}