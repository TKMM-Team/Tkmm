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
        
        var selectedOptions = Options.ToDictionary(option => option.DefaultValue, option => option.SelectedValue);
        var configContent = GenerateConfigContent(selectedOptions);
        
        await File.WriteAllTextAsync(outputPath, configContent);
    }

    private string GenerateConfigContent(Dictionary<string, string> selectedOptions)
    {
        return $@"
[Console]
connect = {selectedOptions["totk console"]}

[Resolution]
MaxFramerate = {selectedOptions["fps"]}
EmuScale = {selectedOptions["emu scale"]}
ShadowResolution = {selectedOptions["shadow resolution"]}
RenderDistance = {selectedOptions["render distance"]}
QualityImprovements = {(selectedOptions["quality improvements"] == "true" ? "On" : "Off")}
RemoveDepthOfField = {(selectedOptions["depthoffield"] == "true" ? "On" : "Off")}
RemoveLensflare = {(selectedOptions["lensflare"] == "true" ? "On" : "Off")}
DisableFXAA = {(selectedOptions["fxaa"] == "true" ? "On" : "Off")}
Width = {selectedOptions["resolution"].Split('x')[0]}
Height = {selectedOptions["resolution"].Split('x')[1]}

[Features]
MenuFPSLock = {selectedOptions["menu fps"]}
MovieFPS = {selectedOptions["movie fps"]}
Fov = {selectedOptions["fov"]}
TimeSpeed = {selectedOptions["time speed"]}
DisableFog = {(selectedOptions["fog"] == "true" ? "On" : "Off")}
IsTimeSlower = {(selectedOptions["slowtime"] == "true" ? "On" : "Off")}

[UltraCam]
FirstPersonFov = 90
CameraSpeed = 30
Speed = 5
AnimationSmoothing = {selectedOptions["ani smoothing"]}
TriggerWithController = {(selectedOptions["freecam"] == "true" ? "On" : "Off")}
FirstPerson = Off
AutoHideUI = {(selectedOptions["hide ui"] == "true" ? "On" : "Off")}
AnimationFadeout = {(selectedOptions["animationfadeout"] == "true" ? "On" : "Off")}

[Gameplay]
Stick_Vertical_Speed = {selectedOptions["vertical r stick"]}
Stick_Horizontal_Speed = {selectedOptions["horizontal r stick"]}
StaminaBar = Off

[Heaps]
RSDB = {selectedOptions["rsdbheap"]}
GameTextures = {selectedOptions["gametexturesheap"]}

[Handheld]
Width = {selectedOptions["handheldwidth"]}
Height = {selectedOptions["handheldheight"]}
OverrideHandheld_Resolution = {(selectedOptions["handheldresolution"] == "true" ? "On" : "Off")}

[Benchmark]
Benchmark = {selectedOptions["benchmark"]}

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
            var defaultValue = option.Value.GetProperty("Default").GetString();
            var values = option.Value.GetProperty("Values").EnumerateArray().Select(v => v.GetString()).ToList();
            var classType = option.Value.GetProperty("Class").GetString();
            options.Add(new OptionModel { DefaultValue = defaultValue, Values = values, SelectedValue = defaultValue, Class = classType });
        }

        return options;
    }
}

public class OptionModel
{
    public string DefaultValue { get; set; }
    public List<string> Values { get; set; }
    public string SelectedValue { get; set; }
    public string Class { get; set; }
}