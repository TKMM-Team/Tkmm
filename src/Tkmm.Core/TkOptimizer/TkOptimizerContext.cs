using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.TkOptimizer.Models;
using Tkmm.Core.TkOptimizer.Models.ValueTypes;
using TkSharp.Core;
using TkSharp.IO.Writers;

namespace Tkmm.Core.TkOptimizer;

/// <summary>
/// TotK optimizer options template.
/// </summary>
public sealed class TkOptimizerContext : ObservableObject
{
    public ObservableCollection<TkOptimizerOptionGroup> Groups { get; } = [];

    public bool IsEnabled {
        get => TkOptimizerStore.Current.IsEnabled;
        set {
            TkOptimizerStore.Current.IsEnabled = value;
            OnPropertyChanged();
            WriteToMergeOutput();
        }
    }

    public string? Preset {
        get => TkOptimizerStore.Current.Preset;
        set {
            TkOptimizerStore.Current.Preset = value;
            OnPropertyChanged();
            WriteToMergeOutput();
        }
    }

    public static TkOptimizerContext Create(Assembly? assembly = null)
    {
        using Stream input = GetOptionsJson(assembly ?? Assembly.GetCallingAssembly());
        var json = JsonSerializer.Deserialize<TkOptimizerJson>(input, TkOptimizerJsonContext.Default.TkOptimizerJson);
        return json is null ? new TkOptimizerContext() : FromJson(json);
    }
    
    private static TkOptimizerContext FromJson(TkOptimizerJson json)
    {
        TkOptimizerContext context = new();

        foreach (IGrouping<string, KeyValuePair<string, TkOptimizerJson.Option>> section in json.Options.GroupBy(x => x.Value.Section)) {
            TkOptimizerOptionGroup group = new(section.Key);
            foreach ((string key, TkOptimizerJson.Option option) in section) {
                var tkOption = TkOptimizerOption.FromJson(key, option);
                tkOption.Value.PropertyChanged += (s, e) => context.WriteToMergeOutput();
                group.Options.Add(tkOption);
            }
            
            context.Groups.Add(group);
        }
        
        return context;
    }

    private static Stream GetOptionsJson(Assembly assembly)
    {
        long plainId = 1;
        Ulid id = Unsafe.As<long, Ulid>(ref plainId);

        Stream? result = null;

        if (TKMM.ModManager.Mods.FirstOrDefault(x => x.Id == id) is { Changelog.Source: ITkSystemSource optimizerSource }) {
            const string target = "extras\\Options.json";
            if (optimizerSource.Exists(target)) {
                result = optimizerSource.OpenRead(target);
            }
        }

        return result ?? assembly.GetManifestResourceStream("Tkmm.Resources.Optimizer.Options.json")!;
    }
    
    public void WriteToMergeOutput()
    {
        string outputFileName = Path.Combine("romfs", "UltraCam",
            // ReSharper disable once StringLiteralTypo
            "maxlastbreath.ini");
        
        FolderModWriter mergeOutput = new(TKMM.MergedOutputFolder);
        using Stream output = mergeOutput.OpenWrite(outputFileName);
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