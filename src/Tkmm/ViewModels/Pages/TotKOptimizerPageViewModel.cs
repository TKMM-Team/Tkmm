using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
    public IRelayCommand GenerateConfigCommand { get; }

    public async Task GenerateConfigFile()
    {
        string outputPath = Path.Combine(TKMM.MergedOutputFolder, ConfigFilePath);
        
        var selectedOptions = Options.ToDictionary(option => option.Name, option => option.SelectedValue);
        string configContent = GenerateConfigContent(selectedOptions);
        
        await File.WriteAllTextAsync(outputPath, configContent);
    }

    private string GenerateConfigContent(Dictionary<string, string> selectedOptions)
    {
        return $@"
[Console]
connect = {selectedOptions["Console IP"]}

[Resolution]
MaxFramerate = {selectedOptions["FPS"]}
EmuScale = {selectedOptions["Emulator Scale"]}
ShadowResolution = {selectedOptions["Shadow Resolution"]}
RenderDistance = {selectedOptions["Render Distance"]}
QualityImprovements = {(selectedOptions["Quality Improvements"] == "true" ? "On" : "Off")}
RemoveDepthOfField = {(selectedOptions["Remove Depth Of Field"] == "true" ? "On" : "Off")}
RemoveLensflare = {(selectedOptions["Remove Lens flare"] == "true" ? "On" : "Off")}
DisableFXAA = {(selectedOptions["Disable Fxaa"] == "true" ? "On" : "Off")}
Width = {selectedOptions["Resolution"].Split('x')[0]}
Height = {selectedOptions["Resolution"].Split('x')[1]}

[Features]
MenuFPSLock = {selectedOptions["Menu FPS"]}
MovieFPS = {selectedOptions["Movie FPS"]}
Fov = {selectedOptions["FOV"]}
TimeSpeed = {selectedOptions["Time Speed"]}
DisableFog = {(selectedOptions["Improve Fog"] == "true" ? "On" : "Off")}
IsTimeSlower = {(selectedOptions["Slow Time"] == "true" ? "On" : "Off")}

[UltraCam]
FirstPersonFov = 90
CameraSpeed = {selectedOptions["Freecam Sensitivity"]}
Speed = {selectedOptions["Freecam Speed"]}
AnimationSmoothing = {selectedOptions["Sequence Smoothing"]}
TriggerWithController = {(selectedOptions["FreeCam"] == "true" ? "On" : "Off")}
FirstPerson = Off
AutoHideUI = {(selectedOptions["Auto Hide UI in FreeCam"] == "true" ? "On" : "Off")}
AnimationFadeout = {(selectedOptions["Last Keyframe Fadeout"] == "true" ? "On" : "Off")}

[Gameplay]
Stick_Vertical_Speed = {selectedOptions["V Camera Sensitivity"]}
Stick_Horizontal_Speed = {selectedOptions["H Camera Sensitivity"]}
StaminaBar = Off

[Heaps]
RSDB = {selectedOptions["RSDB Heap size"]}
GameTextures = {selectedOptions["Textures Heap size"]}

[Handheld]
Width = {selectedOptions["Handheld Width"]}
Height = {selectedOptions["Handheld Height"]}
OverrideHandheld_Resolution = {(selectedOptions["Override Handheld Res"] == "true" ? "On" : "Off")}

[Benchmark]
Benchmark = {selectedOptions["Benchmark Selection"]}

[Randomizer]
IsEnabled = Off
Enemies = Off
Items = Off
Weapons = Off
";
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
            // Read the human-readable Name from JSON.
            string name = option.Value.GetProperty("Name").GetString();
            string classType = option.Value.GetProperty("Class").GetString();

            // Get the "Default" JSON element.
            JsonElement defaultElem = option.Value.GetProperty("Default");
            string defaultValue = defaultElem.ToString();

            // Load values list if defined.
            List<string> values;
            if (option.Value.TryGetProperty("Values", out JsonElement valuesElem) && valuesElem.ValueKind == JsonValueKind.Array)
            {
                values = valuesElem.EnumerateArray().Select(v => v.ToString()).ToList();
            }
            else
            {
                values = new List<string>();
            }

            // For dropdown types, treat the default as an index.
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

            options.Add(new OptionModel
            {
                Name = name,
                DefaultValue = defaultValue,
                Values = values,
                SelectedValue = selectedValue,
                Class = classType
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
}