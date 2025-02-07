using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.TkOptimizer.Models.ValueTypes;

namespace Tkmm.Core.TkOptimizer;

public sealed class TkOptimizerJson
{
    [JsonPropertyName("Keys")]
    public required Dictionary<string, Option> Options { get; init; }

    public sealed class Option
    {
        public required string Name { get; init; }

        public required string Class { get; init; }

        public required string Section { get; init; }

        [JsonPropertyName("Name_Values")]
        public List<string>? NameValues { get; init; }

        public List<JsonElement>? Values { get; init; }

        public string? Type { get; set; }

        public JsonElement Default { get; init; }

        public JsonElement Increments { get; init; }

        public required string Description { get; init; }

        [JsonPropertyName("Config_Class")]
        public required List<string> ConfigClass { get; init; }

        public List<TkOptimizerEnum> GetEnumValues()
        {
            if (NameValues is null || Values is null || NameValues.Count != Values.Count) {
                return [];
            }

            return [
                ..
                NameValues.Select((name, i) => new TkOptimizerEnum(name, Values[i]))
            ];
        }
    }
}

[JsonSerializable(typeof(TkOptimizerJson))]
public partial class TkOptimizerJsonContext : JsonSerializerContext;