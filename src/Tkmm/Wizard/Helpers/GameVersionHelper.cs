using Tkmm.Core;
using Tkmm.Core.Helpers;
using Tkmm.Wizard.Models;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Wizard.Helpers;

internal static class GameVersionHelper
{
    public static bool ShouldShowPreferredVersion(out IReadOnlyList<string> versions, out bool showEmulatorMissingFooter)
    {
        versions = TkConfig.Shared.AvailableUpdateVersions;
        showEmulatorMissingFooter = false;

#if !SWITCH
        if (!string.IsNullOrWhiteSpace(Config.Shared.EmulatorPath) && IsEmulatorUpdateResolved()) {
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

        showEmulatorMissingFooter = true;
        return true;
#endif
    }

    public static StepResult AfterDump(bool offerPreferredVersion = true)
    {
        if (offerPreferredVersion && ShouldShowPreferredVersion(out _, out _)) {
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

#if !SWITCH
    private static bool IsEmulatorUpdateResolved()
    {
        var emulatorFilePath = Config.Shared.EmulatorPath;
        if (string.IsNullOrWhiteSpace(emulatorFilePath)) {
            return false;
        }

        var exeName = Path.GetFileNameWithoutExtension(emulatorFilePath);
        if (exeName.Equals("ryujinx", StringComparison.OrdinalIgnoreCase)) {
            return TkRyujinxHelper.GetSelectedUpdatePath(emulatorFilePath) is { } updatePath
                   && File.Exists(updatePath);
        }

        if (TkEmulatorHelper.GetNandPath(emulatorFilePath) is not { } nandPath || !Directory.Exists(nandPath)) {
            return false;
        }

        if (string.IsNullOrWhiteSpace(TkConfig.Shared.KeysFolderPath)
            || TkKeyUtils.GetKeysFromFolder(TkConfig.Shared.KeysFolderPath) is not { } keys) {
            return false;
        }

        try {
            _ = TkNandUtils.IsValid(keys, nandPath, out var hasUpdate);
            return hasUpdate;
        }
        catch {
            return false;
        }
    }
#endif
}
