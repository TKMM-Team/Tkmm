using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Wizard.Pages;

public enum EmulatorSelection
{
    Ryujinx = 1,
    Switch = 2,
    Other = 3,
    Manual = 4
}

public sealed partial class EmulatorSelectionPageContext : ObservableObject
{
    [ObservableProperty]
    private bool _isRyujinx = true;
    
    [ObservableProperty]
    private bool _isIntelMac = RuntimeInformation.OSArchitecture is Architecture.X64 && OperatingSystem.IsMacOS();

    [ObservableProperty]
    private bool _isSwitch;

    [ObservableProperty]
    private bool _isOtherEmulator;

    [ObservableProperty]
    private bool _isManual;

    public bool IsValid => IsRyujinx || IsSwitch || IsOtherEmulator || IsManual;

    public EmulatorSelection GetSelection()
    {
        if (IsRyujinx) {
            return EmulatorSelection.Ryujinx;
        }
        
        if (IsOtherEmulator) {
            return EmulatorSelection.Other;
        }
        
        return IsSwitch ? EmulatorSelection.Switch : EmulatorSelection.Manual;
    }
}