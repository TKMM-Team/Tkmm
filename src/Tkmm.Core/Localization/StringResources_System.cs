namespace Tkmm.Core.Localization;

// ReSharper disable InconsistentNaming
public class StringResources_System
{
    private const string GROUP = "System";

    public string Notice { get; } = GetStringResource(GROUP, nameof(Notice));
    public string DefaultProfileName { get; } = GetStringResource(GROUP, nameof(DefaultProfileName));
    public string UndefinedVersion { get; } = GetStringResource(GROUP, nameof(UndefinedVersion));
    public string WizPage0_Title { get; } = GetStringResource(GROUP, nameof(WizPage0_Title));
    public string WizPage0_Description { get; } = GetStringResource(GROUP, nameof(WizPage0_Description));
    public string WizPage0_Action { get; } = GetStringResource(GROUP, nameof(WizPage0_Action));
    public string WizPage1_Content { get; } = GetStringResource(GROUP, nameof(WizPage1_Content));
    public string WizPage1_ChooseEmulator { get; } = GetStringResource(GROUP, nameof(WizPage1_ChooseEmulator));
    public string WizPage1_ChooseSwitchConsole { get; } = GetStringResource(GROUP, nameof(WizPage1_ChooseSwitchConsole));
    public string WizPage1_ChooseBoth { get; } = GetStringResource(GROUP, nameof(WizPage1_ChooseBoth));
}
