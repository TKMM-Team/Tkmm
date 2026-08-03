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

    public static string Patch(string existingContent, Dictionary<string, Dictionary<string, string>> updates)
    {
        var pending = CloneUpdates(updates);
        var lines = existingContent.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').ToList();
        var newline = existingContent.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";

        string? currentSection = null;
        for (var i = 0; i < lines.Count; i++) {
            var rawLine = lines[i];
            var trimmed = rawLine.Trim();

            if (trimmed.Length == 0 || trimmed.StartsWith(';') || trimmed.StartsWith('#')) {
                continue;
            }

            if (trimmed.StartsWith('[') && trimmed.EndsWith(']') && trimmed.Length > 2) {
                i += InsertPendingKeys(lines, i, currentSection, pending);
                currentSection = trimmed[1..^1].Trim();
                continue;
            }

            if (currentSection is null || !pending.TryGetValue(currentSection, out var sectionUpdates)) {
                continue;
            }

            var separatorIndex = rawLine.IndexOf('=');
            if (separatorIndex <= 0) {
                continue;
            }

            var key = rawLine[..separatorIndex].Trim();
            if (!sectionUpdates.Remove(key, out var newValue)) {
                continue;
            }

            if (sectionUpdates.Count == 0) {
                pending.Remove(currentSection);
            }

            lines[i] = ReplaceValue(rawLine, separatorIndex, newValue);
        }

        InsertPendingKeys(lines, lines.Count, currentSection, pending);
        AppendRemainingSections(lines, pending);
        return string.Join(newline, lines);
    }

    private static Dictionary<string, Dictionary<string, string>> CloneUpdates(
        Dictionary<string, Dictionary<string, string>> updates)
    {
        Dictionary<string, Dictionary<string, string>> clone = new(StringComparer.OrdinalIgnoreCase);
        foreach (var (section, values) in updates) {
            clone[section] = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);
        }

        return clone;
    }

    private static int InsertPendingKeys(List<string> lines, int index, string? section,
        Dictionary<string, Dictionary<string, string>> pending)
    {
        if (section is null || !pending.Remove(section, out var keys) || keys.Count == 0) {
            return 0;
        }

        var inserted = 0;
        foreach (var (key, value) in keys) {
            lines.Insert(index + inserted, $"{key} = {value}");
            inserted++;
        }

        return inserted;
    }

    private static void AppendRemainingSections(List<string> lines,
        Dictionary<string, Dictionary<string, string>> pending)
    {
        foreach (var (section, values) in pending) {
            if (values.Count == 0) {
                continue;
            }

            if (lines.Count > 0 && lines[^1].Length > 0) {
                lines.Add(string.Empty);
            }

            lines.Add($"[{section}]");
            foreach (var (key, value) in values) {
                lines.Add($"{key} = {value}");
            }
        }
    }

    private static string ReplaceValue(string rawLine, int separatorIndex, string newValue)
    {
        var afterEquals = rawLine[(separatorIndex + 1)..];
        var commentIndex = afterEquals.IndexOf('#');
        var suffix = commentIndex >= 0 ? afterEquals[commentIndex..] : string.Empty;
        var prefix = rawLine[..(separatorIndex + 1)];

        if (suffix.Length > 0) {
            return $"{prefix} {newValue} {suffix.TrimStart()}";
        }

        return $"{prefix} {newValue}";
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