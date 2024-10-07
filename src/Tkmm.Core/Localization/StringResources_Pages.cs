namespace Tkmm.Core.Localization;

public class StringResources_Pages
{
    private const string GROUP = "Pages";

    public string Home { get; } = GetStringResource(GROUP, nameof(Home));
    public string HomeDescription { get; } = GetStringResource(GROUP, nameof(HomeDescription));

    public string Profiles { get; } = GetStringResource(GROUP, nameof(Profiles));
    public string ProfilesDescription { get; } = GetStringResource(GROUP, nameof(ProfilesDescription));

    public string Tools { get; } = GetStringResource(GROUP, nameof(Tools));
    public string ToolsDescription { get; } = GetStringResource(GROUP, nameof(ToolsDescription));

    public string ShopParam { get; } = GetStringResource(GROUP, nameof(ShopParam));
    public string ShopParamDescription { get; } = GetStringResource(GROUP, nameof(ShopParamDescription));

    public string GbMods { get; } = GetStringResource(GROUP, nameof(GbMods));
    public string GbModsDescription { get; } = GetStringResource(GROUP, nameof(GbModsDescription));

    public string Logs { get; } = GetStringResource(GROUP, nameof(Logs));
    public string LogsDescription { get; } = GetStringResource(GROUP, nameof(LogsDescription));

    public string Settings { get; } = GetStringResource(GROUP, nameof(Settings));
    public string SettingsDescription { get; } = GetStringResource(GROUP, nameof(SettingsDescription));
}
