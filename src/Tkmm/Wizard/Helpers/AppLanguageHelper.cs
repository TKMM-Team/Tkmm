using Tkmm.Components;
using Tkmm.Core;

namespace Tkmm.Wizard.Helpers;

public static class AppLanguageHelper
{
    private static string SkipFlagPath
        => Path.Combine(TKMM.BaseDirectory, ".skip-language-setup");

    public static bool CheckSkipApplicationLanguageFlag()
    {
        if (!File.Exists(SkipFlagPath)) {
            return false;
        }

        File.Delete(SkipFlagPath);
        return true;
    }

    public static void RequestRestartToApplyLanguage()
    {
        Config.Shared.Save();
        File.WriteAllText(SkipFlagPath, string.Empty);
#if SWITCH
        Environment.Exit(0);
#else
        AppUpdater.Restart();
#endif
    }
}