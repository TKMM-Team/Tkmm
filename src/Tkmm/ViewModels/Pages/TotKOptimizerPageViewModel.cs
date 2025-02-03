using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;
using System.Collections.ObjectModel;

namespace Tkmm.ViewModels.Pages;

public partial class TotKOptimizerPageViewModel : ObservableObject
{
    private const string ConfigFilePath = "romfs/UltraCam/maxlastbreath.ini";
    private readonly Dictionary<string, string> _options;
    public ObservableCollection<OptionModel> Options { get; set; }

    public TotKOptimizerPageViewModel()
    {
        _options = LoadOptions();
        Options = new ObservableCollection<OptionModel>();
        LoadOptions();
    }

    public Dictionary<string, string> Options => _options;

    public async Task GenerateConfigFile(Dictionary<string, string> selectedOptions)
    {
        string outputPath = Path.Combine(TKMM.MergedOutputFolder, ConfigFilePath);
        
        // Generate the configuration content based on selected options
        var configContent = GenerateConfigContent(selectedOptions);
        
        // Write to the file
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

    private Dictionary<string, string> LoadOptions()
    {
        var options = new Dictionary<string, string>();
        string resourcePath = "Tkmm.Resources.Optimizer.Options.json"; // Path to the Options.json file

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
            options[option.Name] = option.Value.GetProperty("Default").GetString();
        }

        return options;
    }

    public class OptionModel
    {
        public string Key { get; set; }
        public string Class { get; set; }
        public List<string> Values { get; set; }
        public string SelectedValue { get; set; }
    }
}