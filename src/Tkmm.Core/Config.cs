using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models;

namespace Tkmm.Core;

public enum GameBananaSortMode
{
    Default,
    New,
    Updated
}

public partial class Config : ConfigModule<Config>
{
    static Config()
    {
        Directory.CreateDirectory(DocumentsFolder);
    }

    public static string DocumentsFolder { get; }
        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TotK Mod Manager");

    private static readonly string _defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tkmm");
    private static readonly string _defaultMergedPath = Path.Combine(DocumentsFolder, "Merged Output");

    public override string Name { get; } = "tkmm";

    public string StaticStorageFolder { get; }
        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tkmm");

    public static Action<string>? SetTheme { get; set; }

    public Config()
    {
        FileInfo configFileInfo = new(LocalPath);
        if (configFileInfo.Exists && configFileInfo.Length == 0) {
            File.Delete(LocalPath);
        }

        OnSaving += CreateExportLocationSymlinks;
    }

    [ObservableProperty]
    [property: Config(
        Header = "Theme",
        Description = "",
        Group = "Application")]
    [property: DropdownConfig("Dark", "Light")]
    private string _theme = "Dark";

    [ObservableProperty]
    [property: Config(
        Header = "Show Console",
        Description = "Show the console window for additional information (restart required)",
        Group = "Application")]
    private bool _showConsole = false;

    [ObservableProperty]
    [property: Config(
        Header = "System Folder",
        Description = "The folder used to store TKMM system files.",
        Group = "Application")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "config-storage-folder",
        Title = "Storage Folder")]
    private string _storageFolder = _defaultPath;

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
    private string _defaultAuthor = Path.GetFileName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    [ObservableProperty]
    [property: Config(
        Header = "Merged Mod Output Folder",
        Description = "The output folder to write the final merged mod to.",
        Group = "Merging")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "config-mrged-output-folder",
        Title = "Merged Mod Output Folder")]
    private string _mergeOutput = _defaultMergedPath;

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
    private ExportLocationCollection _exportLocations = [
        new() {
            SymlinkPath = Path.Combine(ReadJapaneseCitrusFruitLoadPath(), "TKMM"),
            IsEnabled = false,
        },
        new() {
            SymlinkPath = _ryujinxPath,
            IsEnabled = false,
        }
    ];

    public static readonly GameBananaSortMode[] GameBananaSortModes = Enum.GetValues<GameBananaSortMode>();

    [ObservableProperty]
    private GameBananaSortMode _gameBananaSortMode = GameBananaSortMode.Default;

    partial void OnThemeChanged(string value)
    {
        SetTheme?.Invoke(value);
    }

    private static readonly string _ryujinxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx", "mods", "contents", "0100f2c0115b6000", "TKMM");

    private static readonly string _japaneseCitrusFruitDefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu", "load", "0100F2C0115B6000");
    private static readonly string _japaneseCitrusFruitConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu", "config", "qt-config.ini");
    private static string ReadJapaneseCitrusFruitLoadPath()
    {
        if (!File.Exists(_japaneseCitrusFruitConfigPath)) {
            return _japaneseCitrusFruitDefaultPath;
        }

        using FileStream fs = File.OpenRead(_japaneseCitrusFruitConfigPath);
        using StreamReader reader = new(fs);

        const string prefix = "load_directory=";

        while (reader.ReadLine() is string line) {
            if (line.StartsWith(prefix)) {
                return Path.Combine(line[prefix.Length..], "0100f2c0115b6000");
            }
        }

        return _japaneseCitrusFruitDefaultPath;
    }

    public void EnsureMergeOutput()
    {
        if (string.IsNullOrEmpty(MergeOutput)) {
            MergeOutput = _defaultMergedPath;
        }
    }

    private bool CreateExportLocationSymlinks()
    {
        return SymlinkHelper.CreateMany(ExportLocations
            .Where(x => x.IsEnabled && (x.IsEnabled = !Path.GetFullPath(x.SymlinkPath).Contains(Path.GetFullPath(MergeOutput), StringComparison.InvariantCultureIgnoreCase)))
            .Select(x => (x.SymlinkPath, MergeOutput))
            .ToArray()
        );
    }
}