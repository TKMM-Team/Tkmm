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
    public string WizPage1_Title { get; } = GetStringResource(GROUP, nameof(WizPage1_Title));
    public string WizPage1_Description { get; } = GetStringResource(GROUP, nameof(WizPage1_Description));
    public string WizPage1_ChooseRyujinx { get; } = GetStringResource(GROUP, nameof(WizPage1_ChooseRyujinx));
    public string WizPage1_ChooseOther { get; } = GetStringResource(GROUP, nameof(WizPage1_ChooseOther));
    public string WizPage1_ChooseSwitchOnly { get; } = GetStringResource(GROUP, nameof(WizPage1_ChooseSwitchOnly));
    public string WizPage1A_Title {get;} = GetStringResource(GROUP, nameof(WizPage1A_Title));
    public string WizPage1A_Description {get;} = GetStringResource(GROUP, nameof(WizPage1A_Description));
    public string WizPage1A_Start {get;} = GetStringResource(GROUP, nameof(WizPage1A_Start));
}
