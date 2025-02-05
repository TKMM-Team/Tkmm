using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core;
using Tkmm.Models;

namespace Tkmm.ViewModels.Pages;

public partial class TotKOptimizerPageViewModel : ObservableObject
{
    private static readonly string UltracamFolder = Path.Combine(AppContext.BaseDirectory, ".data", "contents", "00000000000000000000000001", "romfs", "UltraCam");
    private const string ConfigFileName = "maxlastbreath.ini";
    private const string OptionsFileName = "Options.json";
    private const string UltracamResourceName = "Tkmm.Resources.Ultracam.tkcl";

    private ObservableCollection<OptimizerOption> _options;
    public ObservableCollection<OptimizerOption> Options => _options;

    public IEnumerable<OptimizerOption> MainOptions =>
        Options.Where(o => o.Section?.ToLowerInvariant() == "main" && !o.Auto);
    public IEnumerable<OptimizerOption> ExtrasOptions =>
        Options.Where(o => o.Section?.ToLowerInvariant() == "extra" && !o.Auto);

    public IRelayCommand InstallUltracamCommand { get; }

    private bool _isUltracamInstalled;
    public bool IsUltracamInstalled
    {
        get => _isUltracamInstalled;
        private set => SetProperty(ref _isUltracamInstalled, value);
    }

    public TotKOptimizerPageViewModel()
    {
        IsUltracamInstalled = Directory.Exists(UltracamFolder);
        if (IsUltracamInstalled)
        {
            _options = LoadOptions();
            SubscribeToOptionChanges();
        }
        else
        {
            _options = new ObservableCollection<OptimizerOption>();
        }
        InstallUltracamCommand = new AsyncRelayCommand(InstallUltracamAsync);
    }

    public async Task GenerateConfigFile()
    {
        string configContent = GenerateConfigContent();
        string mergedOutputPath = Path.Combine(TKMM.MergedOutputFolder, "romfs", "UltraCam", ConfigFileName);

        if (File.Exists(mergedOutputPath)) {
            await File.WriteAllTextAsync(mergedOutputPath, configContent);
        }

        string ultracamIniPath = Path.Combine(UltracamFolder, ConfigFileName);
        Directory.CreateDirectory(UltracamFolder);
        await File.WriteAllTextAsync(ultracamIniPath, configContent);
    }

    private string GenerateConfigContent()
    {
        var sectionLines = new Dictionary<string, List<string>>();

        foreach (var option in Options)
        {
            if (option.ConfigClass.Count < 2)
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
        foreach (var pair in sectionLines) {
            sb.AppendLine($"[{pair.Key}]");
            foreach (var line in pair.Value) {
                sb.AppendLine(line);
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private string FormatOptionValue(OptimizerOption option)
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

    private ObservableCollection<OptimizerOption> LoadOptions()
    {
        string optionsPath = Path.Combine(UltracamFolder, OptionsFileName);
        if (!File.Exists(optionsPath)) {
            throw new FileNotFoundException($"Options file not found at {optionsPath}");
        }

        using var reader = new StreamReader(optionsPath);
        string json = reader.ReadToEnd();
        var options = new ObservableCollection<OptimizerOption>();
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

            List<string> nameValues = new List<string>();
            if (option.Value.TryGetProperty("Name_Values", out JsonElement nameValuesElem) &&
                nameValuesElem.ValueKind == JsonValueKind.Array)
            {
                nameValues = nameValuesElem.EnumerateArray().Select(e => e.ToString()).ToList();
            }

            string selectedValue = defaultValue;
            int dropdownIndex = 0;
            if (classType.ToLowerInvariant() == "dropdown")
            {
                if (defaultElem.ValueKind == JsonValueKind.Number && int.TryParse(defaultElem.GetRawText(), out int index))
                {
                    dropdownIndex = index;
                    selectedValue = (index >= 0 && index < values.Count) ? values[index] : defaultValue;
                }
                else
                {
                    selectedValue = (values.Count > 0) ? values[0] : defaultValue;
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

            bool auto = false;
            if (option.Value.TryGetProperty("Auto", out JsonElement autoElem) && autoElem.ValueKind == JsonValueKind.True)
            {
                auto = true;
            }
            else if (option.Value.TryGetProperty("Auto", out autoElem) && autoElem.ValueKind == JsonValueKind.False)
            {
                auto = false;
            }

            var optimizerOption = new OptimizerOption
            {
                Name = name,
                DefaultValue = defaultValue,
                Values = values,
                NameValues = nameValues,
                SelectedValue = selectedValue,
                SelectedIndex = (classType.ToLowerInvariant() == "dropdown") ? dropdownIndex : 0,
                Class = classType,
                Section = section,
                Increments = increments,
                ConfigClass = configClass,
                Auto = auto
            };

            options.Add(optimizerOption);
        }

        return options;
    }

    private void SubscribeToOptionChanges()
    {
        foreach (var option in Options)
        {
            option.PropertyChanged += Option_PropertyChanged;
        }
    }

    private void Option_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _ = GenerateConfigFile();
    }

    private async Task InstallUltracamAsync()
    {
        var assembly = typeof(TotKOptimizerPageViewModel).Assembly;
        using var stream = assembly.GetManifestResourceStream(UltracamResourceName);
        
        if (stream == null)
        {
            throw new FileNotFoundException($"Resource {UltracamResourceName} not found.");
        }
        
        await TKMM.Install("Ultracam.tkcl", stream);

        IsUltracamInstalled = Directory.Exists(UltracamFolder);
        
        if (IsUltracamInstalled)
        {
            _options = LoadOptions();
            SubscribeToOptionChanges();
            OnPropertyChanged(nameof(Options));
            OnPropertyChanged(nameof(MainOptions));
            OnPropertyChanged(nameof(ExtrasOptions));
            _ = GenerateConfigFile();
        }
    }
}