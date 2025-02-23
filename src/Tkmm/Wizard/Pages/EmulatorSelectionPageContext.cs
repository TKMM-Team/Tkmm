using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Wizard.Pages;

public enum EmulatorSelection
{
    Ryujinx = 1,
    Switch = 2,
    Other = 3
}

public sealed partial class EmulatorSelectionPageContext : ObservableObject
{
    [ObservableProperty]
    private bool _isRyujinx = true;

    [ObservableProperty]
    private bool _isSwitch;

    [ObservableProperty]
    private bool _isOtherEmulator;

    public bool IsValid => IsRyujinx || IsSwitch || IsOtherEmulator;

    public EmulatorSelection GetSelection()
    {
        if (IsRyujinx) {
            return EmulatorSelection.Ryujinx;
        }
        
        if (IsOtherEmulator) {
            return EmulatorSelection.Other;
        }
        
        if (IsSwitch) {
            return EmulatorSelection.Switch;
        }

        return 0;
    }
}