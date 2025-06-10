using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.Core;

public sealed partial class Config : ConfigModule<Config>
{
    [JsonIgnore]
    public override string Name => "tkmm2";

    public event Action<string> ThemeChanged = delegate { };

    [JsonIgnore]
    public Func<List<string>>? GetLanguages { get; set; }

    [JsonIgnore]
    public GameLanguage[] GameLanguages { get; set; } = [
        new("USen", "American English"),
        new("EUen", "British English"),
        new("JPja", "日本語"),
        new("EUfr", "Français (France)"),
        new("USfr", "Français canadien"),
        new("USes", "Español (América)"),
        new("EUes", "Español (Europa)"),
        new("USpt", "Português Brasileiro"),
        new("EUde", "Deutsch"),
        new("EUnl", "Nederlands"),
        new("EUit", "Italiano"),
        new("EUru", "Русский"),
        new("KRko", "한국어"),
        new("CNzh", "简体中文"),
        new("TWzh", "繁體中文 (台灣)")
    ];

    public static void SaveAll()
    {
        TkConfig.Shared.Save();
        Shared.Save();
    }

    public Config()
    {
        FileInfo configFileInfo = new(LocalPath);
        if (configFileInfo is { Exists: true, Length: 0 }) {
            File.Delete(LocalPath);
        }

        _gameLanguage = GameLanguages[0];
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
        Header = "System Language",
        Description = "The language to use in the user interface (restart required)",
        Group = "Application")]
    [property: DropdownConfig(
        DisplayMemberPath = nameof(SystemLanguage.DisplayName),
        RuntimeItemsSourceMethodName = nameof(GetLanguagesInternal))]
    private SystemLanguage _cultureName = "en_US";

    [ObservableProperty]
    [property: Config(
        Header = "Auto Save Settings",
        Description = "Automatically save the settings when a change is made and there are no errors.",
        Group = "Application")]
    private bool _autoSaveSettings = true;

#if SWITCH
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public string SevenZipPath => "/usr/bin/7zz";
#else
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
#endif

#if !SWITCH
    [property: Config(
        Header = "Emulator Executable Path",
        Description = "The absolute path to your emulator's executable.",
        Group = "Application")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        InstanceBrowserKey = "emulator-path",
#if TARGET_WINDOWS
        Filter = "Executable:*.exe|All files:*",
#else
        Filter = "Executable:*",
#endif
        Title = "Select emulator executable")]
    [ObservableProperty]
    private string? _emulatorPath;
#endif

    [ObservableProperty]
    [property: Config(
        Header = "Show Trivia Popup",
        Description = "Show the trivia popup when merging.",
        Group = "Application")]
    private bool _showTriviaPopup = true;

    [ObservableProperty]
    [property: Config(
        Header = "Default Author",
        Description = "The default author used when packaging TKCL mods.",
        Group = "Packaging")]
    private string _defaultAuthor = string.Empty;

    [ObservableProperty]
    [property: Config(
        Header = "Target Language",
        Description = "The target language that MalsMerger should create an archive for.",
        Group = "Merging")]
    [property: DropdownConfig(
        DisplayMemberPath = "DisplayName",
        RuntimeItemsSourceMethodName = nameof(GetGameLanguages))]
    private GameLanguage _gameLanguage;

#if !SWITCH
    [ObservableProperty]
    [property: Config(
        Header = "Export Locations",
        Description = "Define custom locations to export the merged mod to.",
        Group = "Merging")]
    private ExportLocations _exportLocations = [];

    [ObservableProperty]
    [property: Config(
        Header = "Merge Output Folder",
        Description = "The location to write the merged output to. (Default location is './Merged' next to the TKMM executable)",
        Group = "Merging")]
    private string? _mergeOutput;
#endif

    [ObservableProperty]
    private GameBananaSortMode _gameBananaSortMode = GameBananaSortMode.Default;

    public bool ConfigExists()
    {
        return File.Exists(LocalPath);
    }

    public GameLanguage[] GetGameLanguages() => GameLanguages;

    public List<SystemLanguage> GetLanguagesInternal() => GetLanguages?.Invoke() switch {
        [..] values => values.Select(x => new SystemLanguage(x)).ToList(),
        null => ["en_US"],
    };

#if !SWITCH
    partial void OnEmulatorPathChanged(string? oldValue, string? newValue)
    {
        if (newValue is null || oldValue is null) {
            return;
        }
        
        if (Path.GetFileNameWithoutExtension(newValue).Equals("ryujinx", StringComparison.InvariantCultureIgnoreCase)) {
            TkRyujinxHelper.UseRyujinx(out _);
        }
        else {
            TkEmulatorHelper.UseEmulator(newValue, out _);
        }
        
        string? oldMergedModPath = TkEmulatorHelper.GetModPath(oldValue);
        if (MergeOutput == oldMergedModPath) {
            MergeOutput = TkEmulatorHelper.GetModPath(newValue);
        }
    }
#endif
}