using Tkmm.Core;
using Tkmm.Wizard.Models;

namespace Tkmm.Wizard.Helpers;

internal static class FlowHelper
{
    public static bool ShouldShowPreferredVersion(out IReadOnlyList<string> versions)
    {
        TkConfig.Shared.RefreshAvailableUpdateVersions();
        versions = TkConfig.Shared.AvailableUpdateVersions;

#if !SWITCH
        if (!string.IsNullOrWhiteSpace(Config.Shared.EmulatorPath) && EmulatorHelper.IsEmulatorUpdateResolved()) {
            // emulator update is already resolved (use Auto and skip the chooser)
            TkConfig.Shared.PreferredGameVersion = TkConfig.DefaultGameVersion;
            return false;
        }

        if (Config.Shared.TkmmMode.IsSwitch) {
            return versions.Count > 1;
        }

        return true;
#else
        return versions.Count > 1;
#endif
    }

    public static StepResult AfterDump(bool offerPreferredVersion = true)
    {
        if (offerPreferredVersion && ShouldShowPreferredVersion(out _)) {
            return StepResult.Next(WizardSteps.PreferredVersion);
        }

#if !SWITCH
        // Firmware is only relevant when using mods on real hardware
        if (Config.Shared.TkmmMode.IsEmulator) {
            return StepResult.Next(WizardSteps.GameLanguage);
        }
#endif
        return StepResult.Next(WizardSteps.Firmware);
    }
}
