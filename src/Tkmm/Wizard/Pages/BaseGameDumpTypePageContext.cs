using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Wizard.Pages;

public sealed partial class BaseGameDumpTypePageContext : ObservableObject
{
    [ObservableProperty]
    private bool _isXciNspSelected;

    [ObservableProperty]
    private bool _isRomfsSelected;

    [ObservableProperty]
    private bool _isSdCardSelected;

    [ObservableProperty]
    private bool _isNandSelected;

    public BaseGameDumpType GetSelectedType()
    {
        if (IsXciNspSelected) {
            return BaseGameDumpType.XciNsp;
        }

        if (IsRomfsSelected) {
            return BaseGameDumpType.Romfs;
        }

        if (IsSdCardSelected) {
            return BaseGameDumpType.SdCard;
        }
        
        return IsNandSelected ? BaseGameDumpType.Nand : BaseGameDumpType.XciNsp; // Default
    }
}

public enum BaseGameDumpType
{
    XciNsp,
    Romfs,
    SdCard,
    Nand
} 