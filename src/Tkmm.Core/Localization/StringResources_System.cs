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
    public string WizPageRyujinxSetup_Title { get; } = GetStringResource(GROUP, nameof(WizPageRyujinxSetup_Title));
    public string WizPageRyujinxSetup_Description { get; } = GetStringResource(GROUP, nameof(WizPageRyujinxSetup_Description));
    public string WizPageRyujinxSetup_Action_Start { get; } = GetStringResource(GROUP, nameof(WizPageRyujinxSetup_Action_Start));
    public string WizPage2_Title { get; } = GetStringResource(GROUP, nameof(WizPage2_Title));
    public string WizPage2_Action_Verify { get; } = GetStringResource(GROUP, nameof(WizPage2_Action_Verify));
    public string WizDumpPage_KeysFolderField_Description { get; } = GetStringResource(GROUP, nameof(WizDumpPage_KeysFolderField_Description));
    public string WizDumpPage_BaseGameFileField_Description { get; } = GetStringResource(GROUP, nameof(WizDumpPage_BaseGameFileField_Description));
    public string WizDumpPage_GameUpdateFileField_Description { get; } = GetStringResource(GROUP, nameof(WizDumpPage_GameUpdateFileField_Description));
    public string WizDumpPage_GameDumpFolderField_Description { get; } = GetStringResource(GROUP, nameof(WizDumpPage_GameDumpFolderField_Description));
    public string WizDumpPage_Action_Browse { get; } = GetStringResource(GROUP, nameof(WizDumpPage_Action_Browse));
    public string WizPageFinal_Title { get; } = GetStringResource(GROUP, nameof(WizPageFinal_Title));
    public string WizPageFinal_Action_Finish { get; } = GetStringResource(GROUP, nameof(WizPageFinal_Action_Finish));
    public string NetworkSettings_WiFiService_Name { get; } = GetStringResource(GROUP, nameof(NetworkSettings_WiFiService_Name));
    public string NetworkSettings_SshService_Name { get; } = GetStringResource(GROUP, nameof(NetworkSettings_SshService_Name));
    public string NetworkSettings_SmbService_Name { get; } = GetStringResource(GROUP, nameof(NetworkSettings_SmbService_Name));
    public string NetworkSettings_Action_RefreshNetworks_Name { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Action_RefreshNetworks_Name));
    public string NetworkSettings_Header_WirelessNetworks { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Header_WirelessNetworks));
    public string NetworkSettings_Action_ConnectNetwork { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Action_ConnectNetwork));
    public string NetworkSettings_Action_DisconnectNetwork { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Action_DisconnectNetwork));
    public string NetworkSettings_Action_ForgetNetwork { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Action_ForgetNetwork));
    public string NetworkSettings_Input_Password_Watermark { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Input_Password_Watermark));
    public string NetworkSettings_Modal_ScanningMessage { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Modal_ScanningMessage));
    public string NetworkSettings_Header_Services { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Header_Services));
    public string NetworkSettings_Header_Properties { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Header_Properties));
    public string NetworkSettings_Header_LocalMacAddressProperty { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Header_LocalMacAddressProperty));
    public string NetworkSettings_Header_IpAddressProperty { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Header_IpAddressProperty));
    public string NetworkSettings_Header_SubnetMaskProperty { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Header_SubnetMaskProperty));
    public string NetworkSettings_Header_GatewayProperty { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Header_GatewayProperty));
    public string NetworkSettings_Header_MacAddressProperty { get; } = GetStringResource(GROUP, nameof(NetworkSettings_Header_MacAddressProperty));
}
