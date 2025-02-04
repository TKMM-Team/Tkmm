using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core;

namespace Tkmm.ViewModels.Pages;

public partial class TotKOptimizerPageViewModel : ObservableObject
{
    private const string ConfigFilePath = "romfs/UltraCam/maxlastbreath.ini";
    private readonly ObservableCollection<OptionModel> _options;

    public TotKOptimizerPageViewModel()
    {
        _options = LoadOptions();
        GenerateConfigCommand = new RelayCommand(async () => await GenerateConfigFile());
    }

    public ObservableCollection<OptionModel> Options => _options;
    public IEnumerable<OptionModel> MainOptions =>
        Options.Where(o => o.Section?.ToLowerInvariant() == "main");
    public IEnumerable<OptionModel> ExtrasOptions =>
        Options.Where(o => o.Section?.ToLowerInvariant() == "extra");

    public IRelayCommand GenerateConfigCommand { get; }

    public async Task GenerateConfigFile()
    {
        string outputPath = Path.Combine(TKMM.MergedOutputFolder, ConfigFilePath);
        
        string? directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        string configContent = GenerateConfigContent();
        
        await File.WriteAllTextAsync(outputPath, configContent);
    }

    private string GenerateConfigContent()
    {
        var sectionLines = new Dictionary<string, List<string>>();

        foreach (var option in Options)
        {
            if (option.ConfigClass == null || option.ConfigClass.Count < 2)
                continue;

            string section = option.ConfigClass[0];
            if (!sectionLines.ContainsKey(section))
            {
                sectionLines[section] = new List<string>();
            }

            if (option.ConfigClass.Count == 2)
            {
                string key = option.ConfigClass[1];
                string value = FormatOptionValue(option);
                sectionLines[section].Add($"{key} = {value}");
            }
            else if (option.ConfigClass.Count == 3)
            {
                string key1 = option.ConfigClass[1];
                string key2 = option.ConfigClass[2];
                if (option.SelectedValue.Contains("x"))
                {
                    var parts = option.SelectedValue.Split('x');
                    if (parts.Length >= 2)
                    {
                        sectionLines[section].Add($"{key1} = {parts[0]}");
                        sectionLines[section].Add($"{key2} = {parts[1]}");
                    }
                }
                else
                {
                    sectionLines[section].Add($"{key1} = {option.SelectedValue}");
                }
            }
        }

        var sb = new StringBuilder();
        foreach (var pair in sectionLines)
        {
            sb.AppendLine($"[{pair.Key}]");
            foreach (var line in pair.Value)
            {
                sb.AppendLine(line);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string FormatOptionValue(OptionModel option)
    {
        if (option.Class.ToLowerInvariant() == "bool")
        {
            return option.SelectedValue.ToLowerInvariant() == "true" ? "On" : "Off";
        }
        else
        {
            return option.SelectedValue;
        }
    }

    private ObservableCollection<OptionModel> LoadOptions()
    {
        var options = new ObservableCollection<OptionModel>();
        string resourcePath = "Tkmm.Resources.Optimizer.Options.json";

        using Stream? stream = typeof(TotKOptimizerPageViewModel).Assembly.GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            throw new FileNotFoundException("Options.json not found.");
        }

        using StreamReader reader = new(stream);
        string json = reader.ReadToEnd();
        var optionsData = JsonDocument.Parse(json).RootElement.GetProperty("Keys");

        foreach (var option in optionsData.EnumerateObject())
        {
            string name = option.Value.GetProperty("Name").GetString();
            string classType = option.Value.GetProperty("Class").GetString();
            string section = option.Value.GetProperty("Section").GetString();

            JsonElement defaultElem = option.Value.GetProperty("Default");
            string defaultValue;
            if (defaultElem.ValueKind == JsonValueKind.Number)
            {
                defaultValue = defaultElem.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                defaultValue = defaultElem.ToString();
            }

            List<string> values;
            if (option.Value.TryGetProperty("Values", out JsonElement valuesElem) &&
                valuesElem.ValueKind == JsonValueKind.Array)
            {
                values = valuesElem.EnumerateArray().Select(v => v.ToString()).ToList();
            }
            else
            {
                values = new List<string>();
            }

            string selectedValue = defaultValue;
            if (classType == "dropdown")
            {
                if (defaultElem.ValueKind == JsonValueKind.Number && int.TryParse(defaultElem.GetRawText(), out int index))
                {
                    if (index >= 0 && index < values.Count)
                    {
                        selectedValue = values[index];
                    }
                }
            }
            else
            {
                selectedValue = defaultValue;
            }

            double increments = 1.0;
            if (option.Value.TryGetProperty("Increments", out JsonElement incElem) &&
                incElem.ValueKind == JsonValueKind.Number)
            {
                increments = incElem.GetDouble();
            }

            List<string> configClass = new List<string>();
            if (option.Value.TryGetProperty("Config_Class", out JsonElement configClassElem) &&
                configClassElem.ValueKind == JsonValueKind.Array)
            {
                configClass = configClassElem.EnumerateArray().Select(e => e.ToString()).ToList();
            }

            options.Add(new OptionModel
            {
                Name = name,
                DefaultValue = defaultValue,
                Values = values,
                SelectedValue = selectedValue,
                Class = classType,
                Section = section,
                Increments = increments,
                ConfigClass = configClass
            });
        }

        return options;
    }
}

public class OptionModel : ObservableObject
{
    public string Name { get; set; }
    public string DefaultValue { get; set; }
    public List<string> Values { get; set; }

    private string selectedValue;
    public string SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }

    public string Class { get; set; }
    public string Section { get; set; }
    public double Increments { get; set; }
    public List<string> ConfigClass { get; set; }
}