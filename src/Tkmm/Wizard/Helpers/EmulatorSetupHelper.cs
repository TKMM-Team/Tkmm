#if !SWITCH
using Tkmm.Core;
using Tkmm.Core.Helpers;

namespace Tkmm.Wizard.Helpers;

public static class EmulatorSetupHelper
{
    public static void ResetDumpConfiguration()
    {
        TkConfig.Shared.ResetGameDumpSettings();
        Config.Shared.MergeOutput = null;
    }

    public static void ApplyFromNameOrPath(string emulatorNameOrPath)
    {
        Config.Shared.EmulatorPath = emulatorNameOrPath;

        if (IsRyujinx(emulatorNameOrPath)) {
            TkRyujinxHelper.UseRyujinx(out _, manualSetup: true);
            return;
        }

        TkEmulatorHelper.UseEmulator(emulatorNameOrPath, out _);
    }

    public static string? TryUseRunningRyujinx()
        => TkRyujinxHelper.UseRyujinx(out _).Case as string;

    private static bool IsRyujinx(string emulatorNameOrPath)
        => Path.GetFileNameWithoutExtension(emulatorNameOrPath)
            .Equals("ryujinx", StringComparison.OrdinalIgnoreCase);
}
#endif