using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using System.Text.Json.Serialization;

namespace Tcml.Core;

public partial class TotkConfig : ConfigModule<TotkConfig>
{
    public override string LocalPath
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Totk", "config.json");

    [ObservableProperty]
    [property: Config(
        Header = "Game Path",
        Description = """
            The absolute path to your TotK RomFS game dump
            (e.g. F:\Games\Totk\RomFS)
    
            Required Files:
            - 'Pack/ZsDic.pack.zs'
            """,
        Category = "TotK")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "totk-config-game-path",
        Title = "TotK RomFS Game Path")]
    private string _gamePath = string.Empty;
    
    [JsonIgnore]
    public string ZsDicPath => Path.Combine(GamePath, "Pack", "ZsDic.pack.zs");

    partial void OnGamePathChanged(string value)
    {
        SetValidation(() => GamePath, value => {
            return value is not null
                && File.Exists(Path.Combine(value, "Pack", "ZsDic.pack.zs"));
        });
    }
}