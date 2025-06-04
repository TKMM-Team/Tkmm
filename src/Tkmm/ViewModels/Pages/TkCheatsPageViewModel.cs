using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;
using Tkmm.Core.Services;
using Tkmm.Core.TkOptimizer.Models;

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
        using var rom = TKMM.GetTkRom();
        var versionString = rom.GameVersion.ToString();
        GameVersion = string.Join(".", versionString.ToCharArray());
        
        OnPropertyChanged(nameof(Cheats));
    }
}