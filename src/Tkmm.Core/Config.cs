using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core.Attributes;
using ConfigFactory.Core;

namespace Tkmm.Core;

public partial class Config : ConfigModule<Config>
{
    private static readonly string _defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tkmm");

    public override string Name { get; } = "tkmm";

    public string StaticStorageFolder { get; }
        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tkmm");

    public static Action<string>? SetTheme { get; set; }

    [ObservableProperty]
    [property: ConfigFactory.Core.Attributes.Config(
        Header = "Theme",
        Description = "",
        Group = "Application")]
    [property: ConfigFactory.Core.Attributes.DropdownConfig("Dark", "Light")]
    private string _theme = "Dark";

    [ObservableProperty]
    [property: ConfigFactory.Core.Attributes.Config(
    Header = "Target Language",
    Description = "The target language that MalsMerger should create an archive for.",
    Group = "Application")]
    [property: ConfigFactory.Core.Attributes.DropdownConfig("USen", "EUen", "JPja", "EUfr", "USfr", "USes", "EUes", "EUde", "EUnl", "EUit", "KRko", "CNzh", "TWzh")]
    private string _gameLanguage = "USen";

    [ObservableProperty]
    [property: ConfigFactory.Core.Attributes.Config(
        Header = "Show Console",
        Description = "Show the console window for additional information (restart required)",
        Group = "Application")]
    private bool _showConsole = false;

    [ObservableProperty]
    [property: ConfigFactory.Core.Attributes.Config(
        Header = "System Folder",
        Description = "The folder used to store TKMM system files.",
        Group = "Application")]
    [property: ConfigFactory.Core.Attributes.BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "config-storage-folder",
        Title = "Storage Folder")]
    private string _storageFolder = _defaultPath;

    partial void OnThemeChanged(string value)
    {
        SetTheme?.Invoke(value);
    }
}
