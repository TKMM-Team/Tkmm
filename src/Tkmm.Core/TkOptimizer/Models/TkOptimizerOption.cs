using Humanizer;
using Tkmm.Core.TkOptimizer.Models.ValueTypes;

namespace Tkmm.Core.TkOptimizer.Models;

public sealed class TkOptimizerOption(string key, string name, string description, List<string> configClass, TkOptimizerValue value)
{
    public string Name => TryGetTranslated($"TkOptimizer_{key.Dehumanize()}_Title", name);

    public string Description => TryGetTranslated($"TkOptimizer_{key.Dehumanize()}_Description", description);

    public List<string> ConfigClass { get; } = configClass;

    public TkOptimizerValue Value { get; } = value;

    public static TkOptimizerOption FromJson(TkOptimizerContext context, string key, TkOptimizerJson.Option option)
    {
        TkOptimizerValue value = (option.Class, option.Type) switch {
            (Class: "dropdown", _)
                => new TkOptimizerEnumValue(context,
                    option.Default.GetInt32(), option.GetEnumValues()),
            (Class: "scale", Type: "s32")
                => new TkOptimizerRangeValue(context, option.Default.GetInt32()) {
                    MinValue = option.Values![0].GetInt32(),
                    MaxValue = option.Values![1].GetInt32(),
                    IncrementSize = option.Increments.GetInt32()
                },
            (Class: "scale", Type: "f32")
                => new TkOptimizerFloatingPointRangeValue(context, option.Default.GetDouble()) {
                    MinValue = option.Values![0].GetDouble(),
                    MaxValue = option.Values![1].GetDouble(),
                    IncrementSize = option.Increments.GetDouble()
                },
            (Class: "bool", _) => new TkOptimizerBoolValue(context, option.Default.GetBoolean()),
            _ => throw new NotSupportedException(
                $"Unsupported configuration type: '{option.Class}'")
        };

        value.Key = key;

        return new TkOptimizerOption(key, option.Name, option.Description, option.ConfigClass, value);
    }

    private static string TryGetTranslated(string localeName, string @default)
    {
        if (Locale[localeName, failSoftly: true] is not { } translated || translated == localeName) {
            return @default;
        }

        return translated;
    }
}