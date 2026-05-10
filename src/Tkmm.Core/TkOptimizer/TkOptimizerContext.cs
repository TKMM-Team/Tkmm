using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using Tkmm.Core.Services;
using Tkmm.Core.TkOptimizer.Models;
using Tkmm.Core.TkOptimizer.Models.ValueTypes;
using TkSharp.Core;
using TkSharp.Core.Models;
using TkSharp.IO.Writers;

namespace Tkmm.Core.TkOptimizer;

/// <summary>
/// TotK optimizer options template.
/// </summary>
public sealed class TkOptimizerContext : ObservableObject
{
    private static readonly SemaphoreSlim ApplyAsyncLock = new(1, 1);
    private readonly Dictionary<string, JsonElement> _optionValues = new(StringComparer.OrdinalIgnoreCase);
    private string? _ephemeralSdCardRootPath;

    private bool HasOutputDestination()
    {
        if (!string.IsNullOrWhiteSpace(TkConfig.Shared.SdCardRootPath)) {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(_ephemeralSdCardRootPath)) {
            return true;
        }

#if !SWITCH
        if (string.IsNullOrWhiteSpace(Config.Shared.EmulatorPath)) {
            return false;
        }
        
        var emulatorSdPath = TkEmulatorHelper.GetSdPath(Config.Shared.EmulatorPath);
        return !string.IsNullOrWhiteSpace(emulatorSdPath);
#endif

    }

    private string? GetSdRootForWrite()
        => !string.IsNullOrWhiteSpace(TkConfig.Shared.SdCardRootPath)
            ? TkConfig.Shared.SdCardRootPath
            : _ephemeralSdCardRootPath;

    [NotNull]
    public TkOptimizerStore? Store {
        get => field ?? TkOptimizerStore.Current;
        set;
    }

    public ObservableCollection<TkOptimizerOptionGroup> Groups { get; } = [];
    
    public ObservableCollection<TkOptimizerCheatGroup> CheatGroups { get; } = [];

    public bool IsEnabled {
        get => TkOptimizerStore.Current.IsEnabled;
        set {
            TkOptimizerStore.Current.IsEnabled = value;
            OnPropertyChanged();
        }
    }

    public string? Preset {
        get => TkOptimizerStore.Current.Preset;
        set {
            TkOptimizerStore.Current.Preset = value;
            OnPropertyChanged();
        }
    }

    public static TkOptimizerContext Create()
    {
        TkOptimizerContext context = new();
        context._optionValues.Clear();
        
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

        return context;
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
                group.Options.Add(TkOptimizerOption.FromJson(context, fileName, key, option));
            }
            
            context.Groups.Add(group);
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

            var iniValues = ParseIni(iniPath);
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
#endif
    }

    private static Dictionary<string, Dictionary<string, string>> ParseIni(string iniPath)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string>? currentSection = null;

        foreach (var rawLine in File.ReadLines(iniPath)) {
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
            currentSection[key] = value;
        }

        return result;
    }

    private static void ApplyIniValue(TkOptimizerContext context, TkOptimizerOption option, string rawValue)
    {
        switch (option.Value) {
            case TkOptimizerBoolValue:
                if (TryParseIniBool(rawValue, out var boolValue)) {
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
                if (TryGetEnumIndex(enumValue, rawValue, out var selectedIndex)) {
                    context.SetOptionValue(option.Value.Key, selectedIndex, writeOutput: false);
                }
                return;
        }
    }

    internal bool TryGetOptionValue<T>(string key, out T value) where T : unmanaged
    {
        if (!_optionValues.TryGetValue(key, out var json)) {
            value = default;
            return false;
        }

        value = json.Deserialize<T>();
        return true;
    }

    internal void SetOptionValue<T>(string key, T value, bool writeOutput = true) where T : unmanaged
    {
        _optionValues[key] = JsonSerializer.SerializeToElement(value);
        if (writeOutput) {
            ApplyToSdCard();
        }
    }

    private static bool TryGetEnumIndex(TkOptimizerEnumValue enumValue, string rawValue, out int selectedIndex)
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

    private static bool TryParseIniBool(string value, out bool result)
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

    public void ApplyToSdCard()
    {
        _ = ApplyToSdCardAsync();
    }

    private async Task ApplyToSdCardAsync()
    {
        try {
            ITkModWriter writer = new FolderModWriter(TKMM.MergedOutputFolder);
            await ApplyAsync(writer, cancellationToken: CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Failed to export TotK Optimizer UltraCam configuration to SD paths.");
        }
    }

    public async ValueTask ApplyAsync(ITkModWriter mergeOutputWriter, TkProfile? profile = null,
        CancellationToken cancellationToken = default)
    {
        _ = mergeOutputWriter;

        await ApplyAsyncLock.WaitAsync(cancellationToken).ConfigureAwait(true);
        try {
            await ApplyCoreAsync(profile, cancellationToken).ConfigureAwait(true);
        }
        finally {
            ApplyAsyncLock.Release();
        }
    }

    private async ValueTask ApplyCoreAsync(TkProfile? profile, CancellationToken cancellationToken)
    {
        if (!TkOptimizerStore.IsProfileEnabled(profile)) {
            return;
        }

        Store = TkOptimizerStore.CreateStore(profile);

#if !SWITCH
        if (!HasOutputDestination()) {
            if (TkOptimizerSdPrompt.RequestSdCardRootAsync is not { } requestRoot) {
                TkLog.Instance.LogWarning(
                    "TotK Optimizer configuration was not written: no SD card root or emulator SD path is configured, and no folder prompt is available.");
                Store = null;
                return;
            }

            var chosen = await requestRoot().ConfigureAwait(true);
            if (chosen is null || string.IsNullOrWhiteSpace(chosen.Path)) {
                TkLog.Instance.LogInformation("TotK Optimizer SD path selection was cancelled; configuration was not written.");
                Store = null;
                return;
            }

            if (chosen.PersistToConfig) {
                TkConfig.Shared.SdCardRootPath = chosen.Path;
                TkConfig.Shared.Save();
                _ephemeralSdCardRootPath = null;
            }
            else {
                _ephemeralSdCardRootPath = chosen.Path;
            }
        }
#endif

        foreach (var optionsByFile in Groups.SelectMany(x => x.Options)
                     .GroupBy(x => x.OutputFileName, StringComparer.OrdinalIgnoreCase)) {
            cancellationToken.ThrowIfCancellationRequested();

            var outputSdFileName = Path.Combine("UltraCam", "TOTK", "Config", $"{optionsByFile.Key}.ini");

            using MemoryStream memoryStream = new();
            await using (StreamWriter writer = new(memoryStream, leaveOpen: true)) {
                WriteConfigContent(writer, optionsByFile);
            }

#if !SWITCH
            if (!string.IsNullOrWhiteSpace(Config.Shared.EmulatorPath))
            {
                var emulatorSdPath = TkEmulatorHelper.GetSdPath(Config.Shared.EmulatorPath);

                if (!string.IsNullOrWhiteSpace(emulatorSdPath)) {
                    var fullPath = Path.Combine(emulatorSdPath, outputSdFileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                    memoryStream.Position = 0;
                    await using var emulatorOutput = File.Create(fullPath);
                    await memoryStream.CopyToAsync(emulatorOutput, cancellationToken);
                }
            }
#endif

            var physicalSdRoot = GetSdRootForWrite();
            if (!string.IsNullOrWhiteSpace(physicalSdRoot)) {
                var fullPath = Path.Combine(physicalSdRoot, outputSdFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                memoryStream.Position = 0;
                await using var sdOutput = File.Create(fullPath);
                await memoryStream.CopyToAsync(sdOutput, cancellationToken);
            }
        }

        Store = null;
    }

    private static void WriteConfigContent(StreamWriter writer, IEnumerable<TkOptimizerOption> options)
    {
        foreach (var sectionOptions in options.GroupBy(x => x.ConfigClass[0])) {
            writer.Write("[");
            writer.Write(sectionOptions.Key);
            writer.WriteLine("]");

            foreach (var option in sectionOptions) {
                if (option.Value is TkOptimizerEnumValue enumValue) {
                    WriteEnumValue(writer, option, enumValue);
                    continue;
                }
                
                var key = option.ConfigClass[1];
                var value = option.Value switch {
                    TkOptimizerBoolValue boolean => boolean.Value ? "On" : "Off",
                    TkOptimizerFloatingPointRangeValue f32 => f32.Value.ToString(CultureInfo.InvariantCulture),
                    TkOptimizerRangeValue s32 => s32.Value.ToString(CultureInfo.InvariantCulture),
                    _ => null
                };

                if (value is null) {
                    continue;
                }
                
                writer.Write(key);
                writer.Write(" = ");
                writer.WriteLine(value);
            }
            
            writer.WriteLine();
        }
    }

    private static void WriteEnumValue(in StreamWriter writer, TkOptimizerOption option, TkOptimizerEnumValue enumValue)
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
            writer.Write(properties[0]);
            writer.Write(" = ");
            writer.WriteLine(s32);
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
            writer.Write(properties[i]);
            writer.Write(" = ");
            writer.WriteLine(value[sections[i]]);
        }
    }

    public void Reload()
    {
        OnPropertyChanged(nameof(IsEnabled));
        OnPropertyChanged(nameof(Preset));
    }
}