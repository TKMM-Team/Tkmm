using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Models;

namespace Tkmm.Wizard.Pages;

public sealed partial class TkmmModeSelectionPageContext : ObservableObject
{
    [ObservableProperty]
    public partial bool IsEmulator { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSwitch { get; set; }

    [ObservableProperty]
    public partial bool IsHybrid { get; set; }

    public bool IsValid => IsEmulator || IsSwitch || IsHybrid;

    public TkmmMode GetSelection()
    {
        if (IsSwitch) {
            return new TkmmMode("Switch");
        }

        return IsHybrid ? new TkmmMode("Hybrid") : new TkmmMode("Emulator");
    }
}