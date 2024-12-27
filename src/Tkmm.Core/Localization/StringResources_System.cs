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
    public string WizPage1_Option_Ryujinx { get; } = GetStringResource(GROUP, nameof(WizPage1_Option_Ryujinx));
    public string WizPage1_Option_Switch { get; } = GetStringResource(GROUP, nameof(WizPage1_Option_Switch));
    public string WizPage1_Option_Other { get; } = GetStringResource(GROUP, nameof(WizPage1_Option_Other));
    public string WizPage1_Action_Next { get; } = GetStringResource(GROUP, nameof(WizPage1_Action_Next));
    public string WizPage1A_Title {get;} = GetStringResource(GROUP, nameof(WizPage1A_Title));
    public string WizPage1A_Description {get;} = GetStringResource(GROUP, nameof(WizPage1A_Description));
    public string WizPage1A_Action_Start {get;} = GetStringResource(GROUP, nameof(WizPage1A_Action_Start));
    public string WizPage2_Title {get;} = GetStringResource(GROUP, nameof(WizPage2_Title));
    public string WizPage2_Action_Verify {get;} = GetStringResource(GROUP, nameof(WizPage2_Action_Verify));
    public string WizDumpPage_KeysFolderField_Description {get;} = GetStringResource(GROUP, nameof(WizDumpPage_KeysFolderField_Description));
    public string WizDumpPage_BaseGameFileField_Description {get;} = GetStringResource(GROUP, nameof(WizDumpPage_BaseGameFileField_Description));
    public string WizDumpPage_GameUpdateFileField_Description {get;} = GetStringResource(GROUP, nameof(WizDumpPage_GameUpdateFileField_Description));
    public string WizDumpPage_GameDumpFolderField_Description {get;} = GetStringResource(GROUP, nameof(WizDumpPage_GameDumpFolderField_Description));
    public string WizDumpPage_Action_Browse {get;} = GetStringResource(GROUP, nameof(WizDumpPage_Action_Browse));
}
