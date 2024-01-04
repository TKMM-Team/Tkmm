using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;

namespace Tcml.Core;

public partial class Config : ConfigModule<Config>
{
    public static Action<string>? SetTheme { get; set; }

    [ObservableProperty]
    [property: Config(
        Header = "Theme",
        Description = "",
        Group = "Application")]
    [property: DropdownConfig("Dark", "Light")]
    private string _theme = "Dark";

    partial void OnThemeChanged(string value)
    {
        SetTheme?.Invoke(value);
    }
}
