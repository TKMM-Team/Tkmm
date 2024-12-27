using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Tkmm.Core.Models;
using TkSharp.Extensions.GameBanana;
using TotkCommon;

namespace Tkmm.Core;

public sealed partial class Config : ConfigModule<Config>
{
    public override string Name => "tkmm";
    
    public event Action<string> ThemeChanged = delegate { };

    public Config()
    {
        FileInfo configFileInfo = new(LocalPath);
        if (configFileInfo is { Exists: true, Length: 0 }) {
            File.Delete(LocalPath);
        }
    }
    
    [ObservableProperty]
    [property: Config(
        Header = "Theme",
        Description = "",
        Group = "Application")]
    [property: DropdownConfig("Dark", "Light")]
    private string _theme = "Dark";

    partial void OnThemeChanged(string value)
    {
        ThemeChanged(value);
    }

    [ObservableProperty]
    [property: Config(
        Header = "Show Console",
        Description = "Show the console window for additional information (restart required)",
        Group = "Application")]
    private bool _showConsole;
    
    [ObservableProperty]
    [property: Config(
        Header = "System Language",
        Description = "The language to use in the user interface (restart required)",
        Group = "Application")]
    [property: DropdownConfig("en-US")]
    private string _cultureName = "en-US";

    [ObservableProperty]
    [property: Config(
        Header = "Auto Save Settings",
        Description = "Automatically save the settings when a change is made and there are no errors.",
        Group = "Application")]
    private bool _autoSaveSettings = true;
    
    [ObservableProperty]
    [property: Config(
        Header = "7z Path",
        Description = "The absolute path to the 7-zip executable used for faster 7z extraction.",
        Group = "Application")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        InstanceBrowserKey = "7z-path",
        Filter = "7z:*7z*",
        Title = "7z Location")]
    private string? _sevenZipPath;
    
    [ObservableProperty]
    [property: Config(
        Header = "Default Author",
        Description = "The default author used when packaging TKCL mods.",
        Group = "Packaging")]
    private string _defaultAuthor = string.Empty;
    
    [JsonIgnore, Config(
         Header = "Game Path",
         Description = "The absolute path to your TotK RomFS game dump.",
         Group = "Merging")]
    public string GamePath {
        get => Totk.Config.GamePath;
        set {
            OnPropertyChanging();
            Totk.Config.GamePath = value;
            OnPropertyChanged();
        }
    }
    
    [ObservableProperty]
    [property: Config(
        Header = "Target Language",
        Description = "The target language that MalsMerger should create an archive for.",
        Group = "Merging")]
    [property: DropdownConfig("USen", "EUen", "JPja", "EUfr", "USfr", "USes", "EUes", "EUde", "EUnl", "EUit", "KRko", "CNzh", "TWzh")]
    private string _gameLanguage = "USen";
    
    [ObservableProperty]
    [property: Config(
        Header = "Export Locations",
        Description = "Define custom locations to export the merged mod to.",
        Group = "Merging")]
    private ExportLocations _exportLocations = [];
    
    [ObservableProperty]
    private GameBananaSortMode _gameBananaSortMode = GameBananaSortMode.Default;

    public bool ConfigExists()
    {
        return File.Exists(LocalPath);
    }
}