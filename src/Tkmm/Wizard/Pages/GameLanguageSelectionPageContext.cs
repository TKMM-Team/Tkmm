using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;

namespace Tkmm.Wizard.Pages;

public partial class GameLanguageSelectionPageContext : ObservableObject
{
    [ObservableProperty]
    private string? _gameVersion;

    public GameLanguageSelectionPageContext()
    {
        using var rom = TKMM.GetTkRom();

        GameVersion = rom.GameVersion switch {
            110 => "1.1.0",
            111 => "1.1.1",
            112 => "1.1.2",
            120 => "1.2.0",
            121 => "1.2.1",
            _ => rom.GameVersion.ToString()
        };
    }
} 