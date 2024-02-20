using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using System.Text.Json.Serialization;

namespace Tkmm.Core;

public partial class TotkConfig : ConfigModule<TotkConfig>
{
    public const string ROMFS = "romfs";
    public const string EXEFS = "exefs";

    public static readonly string[] FileSystemFolders = [
        ROMFS,
        EXEFS
    ];

    public override string LocalPath
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Totk", "config.json");

    [ObservableProperty]
    [property: ConfigFactory.Core.Attributes.Config(
        Header = "Game Path",
        Description = """
            The absolute path to your TotK RomFS game dump
            (e.g. F:\Games\Totk\RomFS)
    
            *Required for merging!
            """,
        Category = "TotK")]
    [property: ConfigFactory.Core.Attributes.BrowserConfig(
        BrowserMode = ConfigFactory.Core.Attributes.BrowserMode.OpenFolder,
        InstanceBrowserKey = "totk-config-game-path",
        Title = "TotK RomFS Game Path")]
    private string _gamePath = string.Empty;

    public static int GetVersion(string romfsFolder, int @default = 100)
    {
        string regionLangMask = Path.Combine(romfsFolder, "System", "RegionLangMask.txt");
        if (File.Exists(regionLangMask)) {
            string[] lines = File.ReadAllLines(regionLangMask);
            if (lines.Length >= 3 && int.TryParse(lines[2], out int value)) {
                return value;
            }
        }

        return @default;
    }

    [JsonIgnore]
    public string ZsDicPath => Path.Combine(GamePath, "Pack", "ZsDic.pack.zs");

    [JsonIgnore]
    public int Version => GetVersion(GamePath);

    partial void OnGamePathChanged(string value)
    {
        Validate(() => GamePath, value => {
            return value is not null
                && File.Exists(Path.Combine(value, "Pack", "ZsDic.pack.zs"));
        });
    }

}