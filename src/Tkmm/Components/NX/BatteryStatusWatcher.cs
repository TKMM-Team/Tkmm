#if SWITCH

using CommunityToolkit.HighPerformance;
using Tkmm.ViewModels;

namespace Tkmm.Components.NX;

public static class BatteryStatusWatcher
{
    private const string CHARGE_LOW = "fa-solid fa-battery-low";
    private const string CHARGE_QUARTER = "fa-solid fa-battery-quarter";
    private const string CHARGE_HALF = "fa-solid fa-battery-half";
    private const string CHARGE_THREE_QUARTERS = "fa-solid fa-battery-three-quarters";
    private const string CHARGE_FULL = "fa-solid fa-battery-full";
    private const string CHARGE_ERROR = "fa-regular fa-battery-exclamation";
    
#if TARGET_NX
    private const string WATCH_DIRECTORY = "/sys/class/power_supply/battery";
    private const string STATUS_FILE_PATH = "/sys/class/power_supply/battery/status";
    private const string CHARGE_FILE_PATH = "/sys/class/power_supply/battery/capacity";
#else
    private const string WATCH_DIRECTORY = "/home/archleaders/tkmm-nx/battery/";
    private const string STATUS_FILE_PATH = "/home/archleaders/tkmm-nx/battery/status";
    private const string CHARGE_FILE_PATH = "/home/archleaders/tkmm-nx/battery/capacity";
#endif
    
    public static void Start()
    {
        ShellViewModel.Shared.BatteryIcon = GetChargeIcon(out int charge);
        ShellViewModel.Shared.BatteryCharge = charge;
        
        _ = Task.Run(() => StartInternal(STATUS_FILE_PATH, ProcessStatusChanged));
        _ = Task.Run(() => StartInternal(CHARGE_FILE_PATH, ProcessChargeChanged));
    }

    private static void StartInternal(string path, Action<Stream> process, CancellationToken ct = default)
    {
        string name = Path.GetFileName(path);
        FileSystemWatcher watcher = new(WATCH_DIRECTORY);
        
        while (true) {
            if (ct.IsCancellationRequested) {
                return;
            }
            
            WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Changed);
            if (result.TimedOut || result.Name != name) {
                continue;
            }

            using FileStream fs = File.OpenRead(path);
            process(fs);
        }
    }

    private static void ProcessStatusChanged(Stream input)
    {
        Span<char> status = stackalloc char[4];
        int read = input.Read(status.Cast<char, byte>()) / 2;

        string chargeIcon = GetChargeIcon(out int charge);
        
        string batteryIcon = status[..read] switch {
            // Unicode representation of "Charging" in UTF8
            "桃牡楧杮" => "fa-solid fa-battery-bolt",
            _ => chargeIcon
        };
        
        ShellViewModel.Shared.BatteryIcon = batteryIcon;
        ShellViewModel.Shared.BatteryCharge = charge;
    }

    private static void ProcessChargeChanged(Stream input)
    {
        ShellViewModel.Shared.BatteryIcon = GetChargeIcon(input, out int charge);
        ShellViewModel.Shared.BatteryCharge = charge;
    }

    private static string GetChargeIcon(out int charge)
    {
        using FileStream fs = File.OpenRead(CHARGE_FILE_PATH);
        return GetChargeIcon(fs, out charge);
    }

    private static unsafe string GetChargeIcon(Stream input, out int charge)
    {
        Span<byte> chargeUtf8 = stackalloc byte[4];
        int read = input.Read(chargeUtf8);

        if (!int.TryParse(chargeUtf8[..read], out charge)) {
            charge = -1;
        }

        return charge switch {
            -1 => CHARGE_ERROR,
            < 8 => CHARGE_LOW,
            < 21 => CHARGE_QUARTER,
            < 41 => CHARGE_HALF,
            < 65 => CHARGE_THREE_QUARTERS,
            < 101 => CHARGE_FULL,
            _ => CHARGE_ERROR,
        };
    }
}
#endif