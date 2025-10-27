using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models;
using Tkmm.Core.TkOptimizer;
using TkSharp.Core;
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
        new("USfr", "Français Canadien"),
        new("USes", "Español (América)"),
        new("EUes", "Español (Europa)"),
        new("USpt", "Português Brasileiro (1.4.0+)"),
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
        Header = "Config_Theme",
        Description = "",
        Group = "ConfigSection_Application")]
    [property: DropdownConfig("Dark", "Light")]
    private string _theme = "Dark";

    partial void OnThemeChanged(string value)
    {
        ThemeChanged(value);
    }

    [ObservableProperty]
    [property: Config(
        Header = "Config_SystemLanguage",
        Description = "Config_SystemLanguageDescription",
        Group = "ConfigSection_Application")]
    [property: DropdownConfig(
        DisplayMemberPath = nameof(SystemLanguage.DisplayName),
        RuntimeItemsSourceMethodName = nameof(GetLanguagesInternal))]
    private SystemLanguage _cultureName = "en_US";

    [ObservableProperty]
    [property: Config(
        Header = "Config_AutoSaveSettings",
        Description = "Config_AutoSaveSettingsDescription",
        Group = "ConfigSection_Application")]
    private bool _autoSaveSettings = true;

#if SWITCH
    // ReSharper disable once MemberCanBeMadeStatic.Global
    [JsonIgnore]
    public string SevenZipPath => "/usr/bin/7zz";

    [JsonIgnore]
    public bool UseRomfslite => TkOptimizerStore.IsProfileEnabled();
#else
    [ObservableProperty]
    [property: Config(
        Header = "Config_SevenZipPath",
        Description = "Config_SevenZipPathDescription",
        Group = "ConfigSection_Application")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        InstanceBrowserKey = "7z-path",
        Filter = "7z:*7z*",
        Title = "Config_SelectSevenZipLocation")]
    private string? _sevenZipPath;
#endif

#if !SWITCH
    [property: Config(
        Header = "Config_EmulatorExecutablePath",
        Description = "Config_EmulatorExecutablePathDescription",
        Group = "ConfigSection_Application")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        InstanceBrowserKey = "emulator-path",
#if TARGET_WINDOWS
        Filter = "Executable:*.exe|All files:*",
#else
        Filter = "Executable:*",
#endif
        Title = "Config_SelectEmulatorExecutable")]
    [ObservableProperty]
    private string? _emulatorPath;
#endif

    [ObservableProperty]
    [property: Config(
        Header = "Config_ShowTriviaPopup",
        Description = "Config_ShowTriviaPopupDescription",
        Group = "ConfigSection_Application")]
    private bool _showTriviaPopup = true;

    [ObservableProperty]
    [property: Config(
        Header = "Config_DefaultAuthor",
        Description = "Config_DefaultAuthorDescription",
        Group = "ConfigSection_Packaging")]
    private string _defaultAuthor = string.Empty;

    [ObservableProperty]
    [property: Config(
        Header = "Config_TargetLanguage",
        Description = "Config_TargetLanguageDescription",
        Group = "ConfigSection_Merging")]
    [property: DropdownConfig(
        DisplayMemberPath = "DisplayName",
        RuntimeItemsSourceMethodName = nameof(GetGameLanguages))]
    private GameLanguage _gameLanguage;

#if !SWITCH
    [ObservableProperty]
    [property: Config(
        Header = "Config_ExportLocations",
        Description = "Config_ExportLocationsDescription",
        Group = "ConfigSection_Merging")]
    private ExportLocations _exportLocations = [];

    [ObservableProperty]
    [property: Config(
        Header = "Config_UseRomfslite",
        Description = "Config_UseRomfsliteDescription",
        Group = "ConfigSection_Merging")]
    private bool _useRomfslite;

    [ObservableProperty]
    [property: Config(
        Header = "Config_MergeOutputFolder",
        Description = "Config_MergeOutputFolderDescription",
        Group = "ConfigSection_Merging")]
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
        
        var oldMergedModPath = TkEmulatorHelper.GetModPath(oldValue);
        if (MergeOutput == oldMergedModPath) {
            MergeOutput = TkEmulatorHelper.GetModPath(newValue);
        }
    }

    partial void OnMergeOutputChanged(string? oldValue, string? newValue)
    {
        if (oldValue == null || newValue == null) {
            return;
        }

        if (Directory.Exists(oldValue)) {
            try {
                if (Directory.Exists(newValue)) {
                    Directory.Delete(newValue, true);
                }

                Directory.Move(oldValue, newValue);
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, string.Format(Locale["Config_ErrorFailedToMoveMergeOutput"], oldValue, newValue));
            }
        }

        if (!Directory.Exists(newValue)) {
            Directory.CreateDirectory(newValue);
        }
    }
    
    partial void OnUseRomfsliteChanged(bool value)
    {
        try {
            var romfsPath = Path.Join(TKMM.MergedOutputFolder, "romfs");
            var romfsLitePath = Path.Join(TKMM.MergedOutputFolder, "romfslite");
            
            switch (value) {
                case true when Directory.Exists(romfsPath) && !Directory.Exists(romfsLitePath) && TkOptimizerStore.IsProfileEnabled():
                    Directory.Move(romfsPath, romfsLitePath);
                    break;
                case false when Directory.Exists(romfsLitePath) && !Directory.Exists(romfsPath):
                    Directory.Move(romfsLitePath, romfsPath);
                    break;
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, Locale["Config_ErrorFailedToRenameRomfslite"]);
        }
        
        if (EmulatorPath != null && TkEmulatorHelper.GetModPath(EmulatorPath) is {} modPath) {
            MergeOutput = modPath;
        }
    }
#endif
    
    public override void Load(ref Config module)
    {
        try {
            base.Load(ref module);
        }
        catch (Exception ex) {
            module = new Config();
            TkLog.Instance.LogError(ex, string.Format(Locale["Config_ErrorFailedToLoadConfig"], nameof(Config)));
        }
    }

    public override string Translate(string input)
    {
        return string.IsNullOrWhiteSpace(input) ? input : Locale[input];
    }
}