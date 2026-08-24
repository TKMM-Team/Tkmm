using Tkmm.Core;
using Tkmm.Wizard.Models;

namespace Tkmm.Wizard.Helpers;

internal static class FlowHelper
{
    public static bool ShouldShowPreferredVersion(out IReadOnlyList<string> versions)
    {
        versions = TkConfig.Shared.AvailableUpdateVersions;

#if !SWITCH
        if (!string.IsNullOrWhiteSpace(Config.Shared.EmulatorPath) && EmulatorHelper.IsEmulatorUpdateResolved()) {
            TkConfig.Shared.PreferredGameVersion = TkConfig.DefaultGameVersion;
            return false;
        }
#endif

        TkConfig.Shared.RefreshAvailableUpdateVersions();
        versions = TkConfig.Shared.AvailableUpdateVersions;

#if SWITCH
        return versions.Count > 1;
#else
        if (Config.Shared.TkmmMode.IsSwitch) {
            return versions.Count > 1;
        }

        return true;
#endif
    }

    public static StepResult AfterDump(bool offerPreferredVersion = true)
    {
        if (offerPreferredVersion && ShouldShowPreferredVersion(out _)) {
            return StepResult.Next(WizardSteps.PreferredVersion);
        }

#if !SWITCH
        return !Config.Shared.TkmmMode.IsEmulator
            ? StepResult.Next(WizardSteps.Firmware)
            : StepResult.Next(WizardSteps.GameLanguage);
#else
        return StepResult.Next(WizardSteps.Firmware);
#endif
    }
}
