using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Models;

namespace Tkmm.Wizard.Pages;

public sealed partial class FirmwareSelectionPageContext : ObservableObject
{
    [ObservableProperty]
    public partial bool IsFirmware19OrLower { get; set; } = true;

    [ObservableProperty]
    public partial bool IsFirmware20OrHigher { get; set; }

    public bool IsValid => IsFirmware19OrLower || IsFirmware20OrHigher;

#if SWITCH
    public static bool ShowEmulatorFirmwareNote => false;
#else
    public static bool ShowEmulatorFirmwareNote => true;
#endif

    public SwitchFirmwareVersion GetSelection()
        => new(IsFirmware20OrHigher ? "Firmware20OrHigher" : "Firmware19OrLower");
}