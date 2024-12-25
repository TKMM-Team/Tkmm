namespace Tkmm.Core.Localization;

// ReSharper disable InconsistentNaming
public class StringResources_System
{
    private const string GROUP = "System";

    public string Notice { get; } = GetStringResource(GROUP, nameof(Notice));
    public string DefaultProfileName { get; } = GetStringResource(GROUP, nameof(DefaultProfileName));
    public string UndefinedVersion { get; } = GetStringResource(GROUP, nameof(UndefinedVersion));
    public string WizPage1_Title { get; } = GetStringResource(GROUP, nameof(WizPage1_Title));
    public string WizPage1_Description { get; } = GetStringResource(GROUP, nameof(WizPage1_Description));
    public string WizPage1_Action { get; } = GetStringResource(GROUP, nameof(WizPage1_Action));
}
