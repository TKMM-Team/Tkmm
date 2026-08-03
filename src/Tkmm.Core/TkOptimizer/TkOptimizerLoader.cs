using System.Globalization;
using System.Text;
using System.Text.Json;
using Tkmm.Core.Helpers;
using Tkmm.Core.Services;
using Tkmm.Core.TkOptimizer.Models;
using Tkmm.Core.TkOptimizer.Models.ValueTypes;
using TkSharp.Core;

namespace Tkmm.Core.TkOptimizer;

internal static class TkOptimizerLoader
{
    public static void Load(TkOptimizerContext context)
    {
        using var optionsJsonStream = GetOptionsJsonStream();
        if (JsonSerializer.Deserialize(optionsJsonStream,
                TkOptimizerJsonContext.Default.DictionaryStringDictionaryStringOption) is { } optionsJson) {
            LoadOptions(context, optionsJson);
            LoadValuesFromIni(context);
        }

        using var cheatsJsonStream = GetCheatsJsonStream();
        if (JsonSerializer.Deserialize<TkOptimizerCheatsJson>(cheatsJsonStream, TkOptimizerCheatsJsonContext.Default.TkOptimizerCheatsJson) is { } cheatsJson) {
            LoadCheats(context, cheatsJson);
        }
    }

    private static void LoadOptions(TkOptimizerContext context,
        Dictionary<string, Dictionary<string, TkOptimizerJson.Option>> optionsByFile)
    {
        var options = optionsByFile
            .SelectMany(file =>
                file.Value.Select(option => (FileName: file.Key, Key: option.Key, Option: option.Value)));

        foreach (var section in options.GroupBy(x => x.Option.Section)) {
            TkOptimizerOptionGroup group = new(section.Key);
            foreach (var (fileName, key, option) in section) {
                var loaded = TkOptimizerOption.FromJson(context, fileName, key, option);
                if (loaded.IsAuto) {
                    context.AutoOptions.Add(loaded);
                    continue;
                }

                group.Options.Add(loaded);
            }

            if (group.Options.Count > 0) {
                context.Groups.Add(group);
            }
        }
    }

    private static void LoadCheats(TkOptimizerContext context, TkOptimizerCheatsJson json)
    {
        foreach (var cheat in json) {
            TkOptimizerCheatGroup group = new(cheat.DisplayVersion);
            foreach (var (name, value) in cheat.Cheats) {
                using MemoryStream ms = new(Encoding.UTF8.GetBytes(value));
                group.Cheats.Add(
                    new TkOptimizerCheat(context, group, name, TkCheat.FromText(ms, cheat.Version))
                );
            }

            context.CheatGroups.Add(group);
        }
    }

    private static Stream GetOptionsJsonStream()
    {
        var id = TkOptimizerService.GetStaticId();

        Stream? result = null;

        if (TKMM.ModManager.Mods.FirstOrDefault(x => x.Id == id) is not { Changelog.Source: { } optimizerSource }) {
            return result ?? typeof(TkOptimizerContext).Assembly
                .GetManifestResourceStream("Tkmm.Core.Resources.Optimizer.Options.json")!;
        }

        const string target = "extras/Options.json";
        if (optimizerSource.Exists(target)) {
            result = optimizerSource.OpenRead(target);
        }

        return result ?? typeof(TkOptimizerContext).Assembly
            .GetManifestResourceStream("Tkmm.Core.Resources.Optimizer.Options.json")!;
    }

    private static Stream GetCheatsJsonStream()
    {
        return typeof(TkOptimizerContext).Assembly
            .GetManifestResourceStream("Tkmm.Core.Resources.Optimizer.Cheats.json")!;
    }

    private static void LoadValuesFromIni(TkOptimizerContext context)
    {
        var configRoot = GetConfigRootFolder();
        if (string.IsNullOrWhiteSpace(configRoot) || !Directory.Exists(configRoot)) {
            return;
        }

        foreach (var optionsByFile in context.Groups.SelectMany(x => x.Options)
                     .GroupBy(x => x.OutputFileName, StringComparer.OrdinalIgnoreCase)) {
            var iniPath = Path.Combine(configRoot, $"{optionsByFile.Key}.ini");
            if (!File.Exists(iniPath)) {
                continue;
            }

            var iniValues = TkOptimizerIni.Parse(iniPath);
            foreach (var option in optionsByFile) {
                if (option.ConfigClass.Count < 2) {
                    continue;
                }

                var section = option.ConfigClass[0];
                var key = option.ConfigClass[1];

                if (!iniValues.TryGetValue(section, out var sectionValues)
                    || !sectionValues.TryGetValue(key, out var rawValue)) {
                    continue;
                }

                ApplyIniValue(context, option, rawValue);
            }
        }
    }

    private static string? GetConfigRootFolder()
    {
        if (!string.IsNullOrWhiteSpace(TkConfig.Shared.SdCardRootPath)) {
            var sdRoot = Path.Combine(TkConfig.Shared.SdCardRootPath, "UltraCam", "TOTK", "Config");

            if (Directory.Exists(sdRoot)) {
                return sdRoot;
            }
        }

#if !SWITCH
        if (string.IsNullOrWhiteSpace(Config.Shared.EmulatorPath)) {
            return null;
        }

        var emulatorSdPath = TkEmulatorHelper.GetSdPath(Config.Shared.EmulatorPath);

        if (string.IsNullOrWhiteSpace(emulatorSdPath)) {
            return null;
        }

        var emuRoot = Path.Combine(emulatorSdPath, "UltraCam", "TOTK", "Config");

        return Directory.Exists(emuRoot) ? emuRoot : null;
#else
        return null;
#endif
    }

    private static void ApplyIniValue(TkOptimizerContext context, TkOptimizerOption option, string rawValue)
    {
        switch (option.Value) {
            case TkOptimizerBoolValue:
                if (TkOptimizerIni.TryParseBool(rawValue, out var boolValue)) {
                    context.SetOptionValue(option.Value.Key, boolValue, writeOutput: false);
                }
                return;
            case TkOptimizerRangeValue:
                if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue)) {
                    context.SetOptionValue(option.Value.Key, intValue, writeOutput: false);
                }
                return;
            case TkOptimizerFloatingPointRangeValue:
                if (double.TryParse(rawValue, NumberStyles.Float | NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture, out var floatValue)) {
                    context.SetOptionValue(option.Value.Key, floatValue, writeOutput: false);
                }
                return;
            case TkOptimizerEnumValue enumValue:
                if (TkOptimizerIni.TryGetEnumIndex(enumValue, rawValue, out var selectedIndex)) {
                    context.SetOptionValue(option.Value.Key, selectedIndex, writeOutput: false);
                }
                return;
        }
    }
}