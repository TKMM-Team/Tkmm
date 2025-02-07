using System.Text.Json;

namespace Tkmm.Core.TkOptimizer;

public class TkOptimizerProfile
{
    public Dictionary<string, JsonElement> Values { get; set; } = [];

    public bool IsEnabled { get; set; } = true;

    public string? Preset { get; set; }
}