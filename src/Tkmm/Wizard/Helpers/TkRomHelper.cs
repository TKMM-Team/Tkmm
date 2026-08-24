using Tkmm.Core;
using Tkmm.Dialogs;

namespace Tkmm.Wizard.Helpers;

internal static class TkRomHelper
{
    public static async ValueTask<(bool Ok, bool IsComplete)> Validate(bool needUpdate = true, bool isRomfs = false)
    {
        var rom = TKMM.TryGetTkRom(out var hasBase, out _, out var error);

        if (rom is not null) {
            return (true, true);
        }

        if (!needUpdate && hasBase) {
            return (true, false);
        }

        var missingUpdate = needUpdate && hasBase;
        var (content, title) = isRomfs
            ? (TkLocale.SetupWizard_GameDumpConfigPage_InvalidConfiguration,
                TkLocale.SetupWizard_GameDumpConfigPage_InvalidConfiguration_Title)
            : missingUpdate
                ? (TkLocale.SetupWizard_UpdateDumpConfigPage_InvalidConfiguration,
                    TkLocale.SetupWizard_UpdateDumpConfigPage_InvalidConfiguration_Title)
                : (TkLocale.SetupWizard_BaseGameDumpConfigPage_InvalidConfiguration,
                    TkLocale.SetupWizard_BaseGameDumpConfigPage_InvalidConfiguration_Title);

        await MessageDialog.Show(Locale[content] + "\n\n" + error, title);

        if (missingUpdate) {
            TkConfig.Shared.PackagedUpdatePaths.Clear();
        }
        else {
            TkConfig.Shared.PackagedBaseGamePaths.Clear();
        }

        TkConfig.Shared.GameDumpFolderPaths.Clear();

        return (false, false);
    }
}
