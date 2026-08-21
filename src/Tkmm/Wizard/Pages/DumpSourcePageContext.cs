using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Wizard.Pages;

public enum DumpSource
{
    Ryujinx = 1,
    Switch = 2,
    Other = 3
}

public sealed partial class DumpSourcePageContext(bool showSwitchOption = true) : ObservableObject
{
    public bool ShowSwitchOption { get; } = showSwitchOption;

    [ObservableProperty]
    public partial bool IsRyujinx { get; set; } = true;

    [ObservableProperty]
    public partial bool IsIntelMac { get; set; } = RuntimeInformation.OSArchitecture is Architecture.X64 && OperatingSystem.IsMacOS();

    [ObservableProperty]
    public partial bool IsSwitch { get; set; }

    [ObservableProperty]
    public partial bool IsOtherEmulator { get; set; }

    public bool IsValid => IsRyujinx || IsOtherEmulator || (ShowSwitchOption && IsSwitch);

    public DumpSource GetSelection()
    {
        if (IsRyujinx) {
            return DumpSource.Ryujinx;
        }

        return IsOtherEmulator ? DumpSource.Other : DumpSource.Switch;
    }

    public static DumpSourcePageContext ForSwitchDumpSource()
    {
        return new DumpSourcePageContext(showSwitchOption: true) {
            IsRyujinx = false,
            IsSwitch = true
        };
    }
}