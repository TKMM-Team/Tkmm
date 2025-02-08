using System.Collections.ObjectModel;

namespace Tkmm.Core.TkOptimizer.Models;

public sealed class TkOptimizerCheatGroup(string version)
{
    public string Version { get; } = version;

    public ObservableCollection<TkOptimizerCheat> Cheats { get; } = [];
}