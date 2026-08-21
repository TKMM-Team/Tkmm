namespace Tkmm.Wizard;

public static class WizardSteps
{
    public const string Done = "done";
    public const string Welcome = "welcome";
    public const string ApplicationLanguage = "appLanguage";
    public const string Firmware = "firmware";
    public const string GameLanguage = "gameLanguage";
#if !SWITCH
    public const string Mode = "mode";
    public const string NxRecommend = "nx";
    public const string DumpSource = "dump";
    public const string Ryujinx = "ryujinx";
    public const string Manual = "manual";
#else
    public const string Wifi = "wifi";
    public const string MissingKeys = "missingKeys";
    public const string VerifyDump = "verifyDump";
#endif
}
