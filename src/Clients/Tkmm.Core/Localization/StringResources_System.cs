namespace Tkmm.Core.Localization;

public class StringResources_System
{
    private const string GROUP = "System";

    public string Notice { get; } = GetStringResource(GROUP, nameof(Notice));
    public string DefaultProfileName { get; } = GetStringResource(GROUP, nameof(DefaultProfileName));
    public string UndefinedVersion { get; } = GetStringResource(GROUP, nameof(UndefinedVersion));
}
