#if !SWITCH
namespace Tkmm.Models;

public class DisplayDisk
{
    public static DisplayDisk ManualSelection { get; } = new("Manual selection");

    public DisplayDisk(DriveInfo driveInfo)
    {
        RootPath = driveInfo.RootDirectory.FullName;
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        DisplayName = $"{driveInfo.VolumeLabel ?? driveInfo.DriveType.ToString()} ({driveInfo.Name})";
    }

    public DisplayDisk(string deviceId, string friendlyName, string mtpRootPath)
    {
        IsMtp = true;
        MtpDeviceId = deviceId;
        MtpDeviceName = friendlyName;
        MtpRootPath = mtpRootPath;
        RootPath = string.Empty;
        DisplayName = string.IsNullOrWhiteSpace(friendlyName)
            ? mtpRootPath.TrimStart('\\')
            : $"{friendlyName}{mtpRootPath}";
    }

    private DisplayDisk(string displayName)
    {
        DisplayName = displayName;
        IsManualSelection = true;
        RootPath = string.Empty;
    }

    public string RootPath { get; }
    public string DisplayName { get; }
    public bool IsManualSelection { get; }
    public bool IsMtp { get; }
    public string? MtpDeviceId { get; }
    public string? MtpDeviceName { get; }
    public string? MtpRootPath { get; }

    public override string ToString() => DisplayName;
}
#endif