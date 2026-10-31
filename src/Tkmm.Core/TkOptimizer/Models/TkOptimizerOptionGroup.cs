using System.Collections.ObjectModel;
using Humanizer;

namespace Tkmm.Core.TkOptimizer.Models;

public sealed class TkOptimizerOptionGroup(string name)
{
    private string Name { get; } = name;
    
    public ObservableCollection<TkOptimizerOption> Options { get; } = [];

    public override string ToString()
    {
        return string.Equals(Name, "cheat", StringComparison.OrdinalIgnoreCase) ? "Cheats" : Name.Humanize();
    }
}