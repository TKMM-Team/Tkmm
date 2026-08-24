using Tkmm.Core;
using Tkmm.Wizard.Models;
using Tkmm.Wizard.Steps;

namespace Tkmm.Wizard;

public static class WizardSteps
{
    public const string Welcome = "welcome";
    public const string ApplicationLanguage = "appLanguage";
#if !SWITCH
    public const string Mode = "mode";
    public const string NxRecommend = "nx";
    public const string DumpSource = "dump";
    public const string Ryujinx = "ryujinx";
    public const string Manual = "manual";
    private const string AFTER_APPLICATION_LANGUAGE = Mode;
#else
    public const string Wifi = "wifi";
    public const string MissingKeys = "missingKeys";
    public const string VerifyDump = "verifyDump";
    private const string AFTER_APPLICATION_LANGUAGE = Wifi;
#endif
    public const string PreferredVersion = "preferredVersion";
    public const string Firmware = "firmware";
    public const string GameLanguage = "gameLanguage";
    public const string Done = "done";

    public static ValueTask<StepResult> Run(string step, SetupWizard wizard) => step switch {
        Welcome => WelcomeAsync(wizard),
        ApplicationLanguage => SharedSteps.ApplicationLanguage(wizard, nextStep: AFTER_APPLICATION_LANGUAGE),
#if !SWITCH
        Mode => DesktopSteps.Mode(wizard),
        NxRecommend => DesktopSteps.NxRecommend(wizard),
        DumpSource => DesktopSteps.DumpSourceStep(wizard),
        Ryujinx => DesktopSteps.Ryujinx(wizard),
        Manual => ManualSteps.Show(wizard),
#else
        Wifi => NxSteps.Wifi(wizard),
        MissingKeys => NxSteps.MissingKeys(wizard),
        VerifyDump => NxSteps.VerifyDump(wizard),
#endif
        Firmware => SharedSteps.Firmware(wizard),
        PreferredVersion => SharedSteps.PreferredVersion(wizard),
        GameLanguage => SharedSteps.GameLanguage(wizard),
        _ => ValueTask.FromResult(StepResult.Done())
    };

    private static async ValueTask<StepResult> WelcomeAsync(SetupWizard wizard)
    {
        await wizard.FirstPage();
        return StepResult.Next(wizard.SkipApplicationLanguage ? AFTER_APPLICATION_LANGUAGE : ApplicationLanguage);
    }
}
