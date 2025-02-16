using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.Mvvm.ComponentModel;
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
    private TkOptimizerStore? _store;

    [NotNull]
    public TkOptimizerStore? Store {
        get => _store ?? TkOptimizerStore.Current;
        set => _store = value;
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
        
        using Stream optionsJsonStream = GetOptionsJsonStream();
        if (JsonSerializer.Deserialize<TkOptimizerJson>(optionsJsonStream, TkOptimizerJsonContext.Default.TkOptimizerJson) is { } optionsJson) {
            LoadOptions(context, optionsJson);
        }
        
        using Stream cheatsJsonStream = GetCheatsJsonStream();
        if (JsonSerializer.Deserialize<TkOptimizerCheatsJson>(cheatsJsonStream, TkOptimizerCheatsJsonContext.Default.TkOptimizerCheatsJson) is { } cheatsJson) {
            LoadCheats(context, cheatsJson);
        }

        return context;
    }
    
    private static void LoadOptions(TkOptimizerContext context, TkOptimizerJson json)
    {
        foreach (IGrouping<string, KeyValuePair<string, TkOptimizerJson.Option>> section in json.Options.GroupBy(x => x.Value.Section)) {
            TkOptimizerOptionGroup group = new(section.Key);
            foreach ((string key, TkOptimizerJson.Option option) in section) {
                group.Options.Add(TkOptimizerOption.FromJson(context, key, option));
            }
            
            context.Groups.Add(group);
        }
    }
    
    private static void LoadCheats(TkOptimizerContext context, TkOptimizerCheatsJson json)
    {
        foreach (TkOptimizerCheatsJson.Cheat cheat in json) {
            TkOptimizerCheatGroup group = new(cheat.DisplayVersion);
            foreach ((string name, string value) in cheat.Cheats) {
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
        Ulid id = TkOptimizerService.GetStaticId();

        Stream? result = null;

        if (TKMM.ModManager.Mods.FirstOrDefault(x => x.Id == id) is { Changelog.Source: ITkSystemSource optimizerSource }) {
            const string target = "extras/Options.json";
            if (optimizerSource.Exists(target)) {
                result = optimizerSource.OpenRead(target);
            }
        }

        return result ?? typeof(TkOptimizerContext).Assembly
            .GetManifestResourceStream("Tkmm.Core.Resources.Optimizer.Options.json")!;
    }

    private static Stream GetCheatsJsonStream()
    {
        return typeof(TkOptimizerContext).Assembly
            .GetManifestResourceStream("Tkmm.Core.Resources.Optimizer.Cheats.json")!;
    }

    public void ApplyToMergedOutput()
    {
        ITkModWriter writer = new FolderModWriter(TKMM.MergedOutputFolder);
        Apply(writer);
    }
    
    public void Apply(ITkModWriter mergeOutputWriter, TkProfile? profile = null)
    {
        Store = TkOptimizerStore.CreateStore(profile);

        string outputFileName = Path.Combine("romfs", "UltraCam",
            // ReSharper disable once StringLiteralTypo
            "maxlastbreath.ini");
        
        using Stream output = mergeOutputWriter.OpenWrite(outputFileName);
        using StreamWriter writer = new(output);

        foreach (IGrouping<string, TkOptimizerOption> options in Groups.SelectMany(x => x.Options).GroupBy(x => x.ConfigClass[0])) {
            writer.Write("[");
            writer.Write(options.Key);
            writer.WriteLine("]");

            foreach (TkOptimizerOption option in options) {
                if (option.Value is TkOptimizerEnumValue enumValue) {
                    WriteEnumValue(writer, option, enumValue);
                    continue;
                }
                
                string key = option.ConfigClass[1];
                string? value = option.Value switch {
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

        Store = null;
    }

    private static void WriteEnumValue(in StreamWriter writer, TkOptimizerOption option, TkOptimizerEnumValue enumValue)
    {
        JsonElement choice = enumValue.Values[enumValue.Value].Value;
        Span<string> properties = option.ConfigClass.AsSpan()[1..];

        if (choice.ValueKind is JsonValueKind.Number && choice.TryGetInt32(out int s32)) {
            writer.Write(properties[0]);
            writer.Write(" = ");
            writer.WriteLine(s32);
            return;
        }

        if (choice.ValueKind is not JsonValueKind.String || choice.GetString() is not { } value) {
            throw new ArgumentException($"Unexpected enum value: {choice}");
        }

        Span<Range> sections = new Range[properties.Length];
        int sectionCount = value.AsSpan().Split(sections, 'x');

        if (sectionCount != sections.Length) {
            throw new ArgumentException($"Unexpected split in '{value}', expected {sections.Length} parts but found {sectionCount}.");
        }

        for (int i = 0; i < properties.Length; i++) {
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