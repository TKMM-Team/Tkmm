using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using Tkmm.Core.TkOptimizer.Models;
using Tkmm.Core.TkOptimizer.Models.ValueTypes;

namespace Tkmm.Core.TkOptimizer;

internal static class TkOptimizerIni
{
    public static Dictionary<string, Dictionary<string, string>> Parse(string iniPath)
    {
        using var reader = File.OpenText(iniPath);
        return Parse(reader);
    }

    public static Dictionary<string, Dictionary<string, string>> Parse(TextReader reader)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string>? currentSection = null;

        while (reader.ReadLine() is { } rawLine) {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#')) {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']') && line.Length > 2) {
                var sectionName = line[1..^1].Trim();
                currentSection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                result[sectionName] = currentSection;
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0 || currentSection is null) {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            var comment = value.IndexOf('#');
            if (comment >= 0) {
                value = value[..comment].Trim();
            }

            currentSection[key] = value;
        }

        return result;
    }

    public static bool TryParseBool(string value, out bool result)
    {
        if (bool.TryParse(value, out result)) {
            return true;
        }

        switch (value.Trim()) {
            case "On":
            case "on":
            case "1":
                result = true;
                return true;
            case "Off":
            case "off":
            case "0":
                result = false;
                return true;
            default:
                result = false;
                return false;
        }
    }

    public static bool TryGetEnumIndex(TkOptimizerEnumValue enumValue, string rawValue, out int selectedIndex)
    {
        selectedIndex = 0;

        if (enumValue.Values.Count == 0) {
            return false;
        }

        if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rawInt)) {
            for (var i = 0; i < enumValue.Values.Count; i++) {
                var value = enumValue.Values[i].Value;

                if (value.ValueKind is not JsonValueKind.Number || !value.TryGetInt32(out var jsonInt) || jsonInt != rawInt) {
                    continue;
                }

                selectedIndex = i;
                return true;
            }
        }

        for (var i = 0; i < enumValue.Values.Count; i++) {
            var value = enumValue.Values[i].Value;
            if (value.ValueKind is not JsonValueKind.String || value.GetString() is not { } jsonString) {
                continue;
            }

            if (!string.Equals(jsonString, rawValue, StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            selectedIndex = i;
            return true;
        }

        return false;
    }

    public static string FormatBool(TkOptimizerOption option, bool value)
    {
        if (string.Equals(option.OutputFileName, "Heap", StringComparison.OrdinalIgnoreCase)) {
            return value ? "True" : "False";
        }

        return value ? "On" : "Off";
    }

    public static string FormatFloat(TkOptimizerOption option, double value)
    {
        if (string.Equals(option.OutputFileName, "Heap", StringComparison.OrdinalIgnoreCase)) {
            return value.ToString("F2", CultureInfo.InvariantCulture);
        }

        return value.ToString(CultureInfo.InvariantCulture);
    }

    public static bool TryFormatOverrideValue(TkOptimizerOption option, string rawValue, [NotNullWhen(true)] out string? formatted)
    {
        formatted = null;
        rawValue = rawValue.Trim();
        if (rawValue.Length == 0) {
            return false;
        }

        switch (option.Value) {
            case TkOptimizerBoolValue:
                if (!TryParseBool(rawValue, out var boolValue)) {
                    return false;
                }

                formatted = FormatBool(option, boolValue);
                return true;
            case TkOptimizerRangeValue:
                if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue)) {
                    return false;
                }

                formatted = intValue.ToString(CultureInfo.InvariantCulture);
                return true;
            case TkOptimizerFloatingPointRangeValue:
                if (!double.TryParse(rawValue, NumberStyles.Float | NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture, out var floatValue)) {
                    return false;
                }

                formatted = FormatFloat(option, floatValue);
                return true;
            case TkOptimizerEnumValue:
                formatted = rawValue;
                return true;
            default:
                return false;
        }
    }
}