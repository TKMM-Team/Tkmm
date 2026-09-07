using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;

namespace Tkmm.Wizard.Pages;

public partial class GameLanguageSelectionPageContext : ObservableObject
{
    [ObservableProperty]
    public partial string? GameVersion { get; set; }

    public GameLanguageSelectionPageContext()
    {
        using var rom = TKMM.GetTkRom();
        var versionString = rom.GameVersion.ToString();
        GameVersion = string.Join(".", versionString.ToCharArray());
    }
} 
