namespace Tkmm.Core.Models;

public class FriendlyDriveInfo(DriveInfo drive)
{
    public DriveInfo Drive { get; } = drive;
    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    public string DisplayName { get; } = $"{drive.VolumeLabel ?? drive.DriveType.ToString()} ({drive.Name})";

    public void Desconstruct(out DriveInfo drive, out string name)
    {
        drive = Drive;
        name = DisplayName;
    }

    public override string ToString()
    {
        return DisplayName;
    }
}
