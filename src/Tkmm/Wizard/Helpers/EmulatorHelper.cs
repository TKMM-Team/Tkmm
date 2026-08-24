#if !SWITCH
using Tkmm.Core;
using Tkmm.Core.Helpers;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Wizard.Helpers;

public static class EmulatorHelper
{
    public static void ResetDumpConfiguration()
    {
        TkConfig.Shared.Reset();
        Config.Shared.MergeOutput = null;
    }

    public static void ApplyFromNameOrPath(string emulatorNameOrPath)
    {
        Config.Shared.EmulatorPath = emulatorNameOrPath;

        if (IsRyujinx(emulatorNameOrPath)) {
            TkRyujinxHelper.UseRyujinx(manualSetup: true);
            return;
        }

        TkEmulatorHelper.UseEmulator(emulatorNameOrPath, out _);
    }

    public static string? TryUseRyujinx()
        => TkRyujinxHelper.UseRyujinx().Case as string;

    private static bool IsRyujinx(string emulatorNameOrPath)
        => Path.GetFileNameWithoutExtension(emulatorNameOrPath)
            .Equals("ryujinx", StringComparison.OrdinalIgnoreCase);

    public static bool IsEmulatorUpdateResolved()
    {
        var emulatorFilePath = Config.Shared.EmulatorPath;
        if (string.IsNullOrWhiteSpace(emulatorFilePath)) {
            return false;
        }

        var exeName = Path.GetFileNameWithoutExtension(emulatorFilePath);
        
        if (exeName.Equals("ryujinx", StringComparison.OrdinalIgnoreCase)) {
            return TkRyujinxHelper.GetSelectedUpdatePath(emulatorFilePath) is { } updatePath && File.Exists(updatePath);
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
}
#endif
