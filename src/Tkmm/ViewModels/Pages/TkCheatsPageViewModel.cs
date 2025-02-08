using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;
using Tkmm.Core.Services;
using Tkmm.Core.TkOptimizer.Models;
using TkSharp.Core;

namespace Tkmm.ViewModels.Pages;

public partial class TkCheatsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _gameVersion;

    // ReSharper disable once MemberCanBeMadeStatic.Global
    public IEnumerable<TkOptimizerCheat>? Cheats
        => TkOptimizerService.Context.CheatGroups.FirstOrDefault(x => x.Version == GameVersion)?.Cheats;

    public void Reload()
    {
        using ITkRom rom = TKMM.GetTkRom();

        GameVersion = rom.GameVersion switch {
            110 => "1.1.0",
            111 => "1.1.1",
            112 => "1.1.2",
            120 => "1.2.0",
            121 => "1.2.1",
            _ => rom.ToString()
        };
        
        OnPropertyChanged(nameof(Cheats));
    }
}