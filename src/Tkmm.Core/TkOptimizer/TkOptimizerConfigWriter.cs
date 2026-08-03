using System.Globalization;
using System.Text.Json;
using CommunityToolkit.HighPerformance;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Services;
using Tkmm.Core.TkOptimizer.Models;
using Tkmm.Core.TkOptimizer.Models.ValueTypes;
using TkSharp;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Core.TkOptimizer;

internal static class TkOptimizerConfigWriter
{
    public static Dictionary<string, Dictionary<string, string>> Build(IEnumerable<TkOptimizerOption> options)
    {
        Dictionary<string, Dictionary<string, string>> config = new(StringComparer.OrdinalIgnoreCase);

        foreach (var sectionOptions in options.GroupBy(x => x.ConfigClass[0])) {
            Dictionary<string, string> sectionValues = new(StringComparer.OrdinalIgnoreCase);
            config[sectionOptions.Key] = sectionValues;

            foreach (var option in sectionOptions) {
                if (option.Value is TkOptimizerEnumValue enumValue) {
                    WriteEnumValue(sectionValues, option, enumValue);
                    continue;
                }
                
                var key = option.ConfigClass[1];
                var value = option.Value switch {
                    TkOptimizerBoolValue boolean => TkOptimizerIni.FormatBool(option, boolean.Value),
                    TkOptimizerFloatingPointRangeValue f32 => TkOptimizerIni.FormatFloat(option, f32.Value),
                    TkOptimizerRangeValue s32 => s32.Value.ToString(CultureInfo.InvariantCulture),
                    _ => null
                };

                if (value is null) {
                    continue;
                }
                
                sectionValues[key] = value;
            }
        }

        return config;
    }

    public static void Write(StreamWriter writer, Dictionary<string, Dictionary<string, string>> config)
    {
        foreach (var (section, values) in config) {
            writer.Write("[");
            writer.Write(section);
            writer.WriteLine("]");

            foreach (var (key, value) in values) {
                writer.Write(key);
                writer.Write(" = ");
                writer.WriteLine(value);
            }
            
            writer.WriteLine();
        }
    }

    public static void ApplyModOverrides(Dictionary<string, Dictionary<string, string>> config, string outputFileName,
        TkOptimizerOption[] fileOptions, TkProfile profile)
    {
        Dictionary<string, Dictionary<string, TkOptimizerOption>> lookup = new(StringComparer.OrdinalIgnoreCase);
        foreach (var option in fileOptions) {
            if (option.ConfigClass.Count < 2) {
                continue;
            }

            if (!lookup.TryGetValue(option.ConfigClass[0], out var keys)) {
                keys = new Dictionary<string, TkOptimizerOption>(StringComparer.OrdinalIgnoreCase);
                lookup[option.ConfigClass[0]] = keys;
            }

            for (var i = 1; i < option.ConfigClass.Count; i++) {
                keys[option.ConfigClass[i]] = option;
            }
        }

        var optimizerId = TkOptimizerService.GetStaticId();
        foreach (var changelog in TkModManager.GetMergeTargets(profile, mod => mod.Mod.Id != optimizerId)) {
            if (changelog.Source is not { } source) {
                continue;
            }

            var relativePath = ResolveExtrasIniPath(source, outputFileName);
            if (relativePath is null) {
                continue;
            }

            Dictionary<string, Dictionary<string, string>> ini;
            try {
                using var stream = source.OpenRead(relativePath);
                using var reader = new StreamReader(stream);
                ini = TkOptimizerIni.Parse(reader);
            }
            catch (Exception ex) {
                TkLog.Instance.LogWarning(ex, "Failed to parse optimizer extras INI '{RelativePath}'", relativePath);
                continue;
            }

            foreach (var (section, values) in ini) {
                if (!lookup.TryGetValue(section, out var sectionOptions)) {
                    TkLog.Instance.LogWarning(
                        "Ignoring section [{Section}] in '{RelativePath}': section is not defined for '{OutputFile}'",
                        section, relativePath, outputFileName);
                    continue;
                }

                if (!config.TryGetValue(section, out var sectionValues)) {
                    sectionValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    config[section] = sectionValues;
                }

                foreach (var (key, rawValue) in values) {
                    if (!sectionOptions.TryGetValue(key, out var option)) {
                        TkLog.Instance.LogWarning(
                            "Ignoring '{Key}' in [{Section}] from '{RelativePath}': option is not defined for '{OutputFile}'",
                            key, section, relativePath, outputFileName);
                        continue;
                    }

                    if (!TkOptimizerIni.TryFormatOverrideValue(option, rawValue, out var formatted)) {
                        TkLog.Instance.LogWarning(
                            "Ignoring '{Key}' in [{Section}] from '{RelativePath}': invalid value '{RawValue}'",
                            key, section, relativePath, rawValue);
                        continue;
                    }

                    sectionValues[key] = formatted;
                }
            }
        }
    }

    private static string? ResolveExtrasIniPath(ITkSystemSource source, string outputFileName)
    {
        foreach (var folder in (string[])["UltraCam", "Ultracam"]) {
            var relativePath = $"extras/{folder}/{outputFileName}.ini";
            if (source.Exists(relativePath)) {
                return relativePath;
            }
        }

        return null;
    }

    private static void WriteEnumValue(Dictionary<string, string> sectionValues, TkOptimizerOption option, TkOptimizerEnumValue enumValue)
    {
        if (enumValue.Values.Count == 0) {
            return;
        }

        var selectedIndex = Math.Clamp(enumValue.Value, 0, enumValue.Values.Count - 1);
        var choice = enumValue.Values[selectedIndex].Value;
        var properties = option.ConfigClass.AsSpan()[1..];

        if (properties.Length == 0) {
            return;
        }

        if (choice.ValueKind is JsonValueKind.Number && choice.TryGetInt32(out var s32)) {
            sectionValues[properties[0]] = s32.ToString(CultureInfo.InvariantCulture);
            return;
        }

        if (choice.ValueKind is not JsonValueKind.String || choice.GetString() is not { } value) {
            throw new ArgumentException($"Unexpected enum value: {choice}");
        }

        Span<Range> sections = new Range[properties.Length];
        var sectionCount = value.AsSpan().Split(sections, 'x');

        if (sectionCount != sections.Length) {
            throw new ArgumentException($"Unexpected split in '{value}', expected {sections.Length} parts but found {sectionCount}.");
        }

        for (var i = 0; i < properties.Length; i++) {
            sectionValues[properties[i]] = value[sections[i]];
        }
    }
}