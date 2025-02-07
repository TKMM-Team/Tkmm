using System.Text.Json;

namespace Tkmm.Core.TkOptimizer.Models.ValueTypes;

public class TkOptimizerEnum(string name, JsonElement value)
{
    public string Name { get; } = name;

    public JsonElement Value { get; } = value;
}