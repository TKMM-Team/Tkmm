namespace Tkmm.Core.TkOptimizer;

public class TkOptimizerProfile
{
    public Dictionary<string, HashSet<string>> Cheats { get; } = [];

    public string? Preset { get; set; }
}