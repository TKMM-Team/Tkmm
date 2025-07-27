using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Wizard.Pages;

public sealed partial class UpdateDumpTypePageContext : ObservableObject
{
    [ObservableProperty]
    private bool _isNspSelected;

    [ObservableProperty]
    private bool _isSdCardSelected;

    [ObservableProperty]
    private bool _isNandSelected;

    public UpdateDumpType GetSelectedType()
    {
        if (IsNspSelected) {
            return UpdateDumpType.Nsp;
        }

        if (IsSdCardSelected) {
            return UpdateDumpType.SdCard;
        }
        
        return IsNandSelected ? UpdateDumpType.Nand : UpdateDumpType.Nsp; // Default
    }
}

public enum UpdateDumpType
{
    Nsp,
    SdCard,
    Nand
} 