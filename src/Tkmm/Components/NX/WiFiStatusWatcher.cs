#if SWITCH

using Tkmm.Core.Services;
using Tkmm.ViewModels;

namespace Tkmm.Components.NX;

public static class WiFiStatusWatcher
{
    private static readonly Timer AutoUpdateWiFiTimer = new(state => {
        _ = Task.Run(UpdateSignal);
    });

    private const string WIFI_DISCONNECTED = "fa-solid fa-wifi-slash";
    private const string WIFI_LOW = "fa-solid fa-wifi-weak";
    private const string WIFI_MEDIUM = "fa-solid fa-wifi-fair";
    private const string WIFI_HIGH = "fa-solid fa-wifi";

    private static bool _isConnected;

    public static void Start()
    {
        SetConnected(false);
    }

    public static void SetConnected(bool isConnected)
    {
        _isConnected = isConnected;

        if (!isConnected) {
            AutoUpdateWiFiTimer.Change(Timeout.Infinite, Timeout.Infinite);
            ShellViewModel.Shared.WiFiIcon = WIFI_DISCONNECTED;
            return;
        }

        UpdateSignal();
        AutoUpdateWiFiTimer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(2));
    }

    private static void UpdateSignal()
    {
        if (!_isConnected) {
            ShellViewModel.Shared.WiFiIcon = WIFI_DISCONNECTED;
            return;
        }

        ShellViewModel.Shared.WiFiIcon = Connman.GetSignalStrength() switch {
            null => WIFI_DISCONNECTED,
            >= -62 => WIFI_HIGH,
            >= -72 => WIFI_MEDIUM,
            _ => WIFI_LOW
        };
    }
}
#endif