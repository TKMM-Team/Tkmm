using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core.Attributes;
using ConfigFactory.Core;

namespace Tcml.Core;

public partial class Config : ConfigModule<Config>
{
    private static readonly string _defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tkmm");

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
