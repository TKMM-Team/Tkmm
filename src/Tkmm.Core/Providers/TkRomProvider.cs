using System.Net.Http.Headers;
using LibHac.Common.Keys;
using LibHac.Tools.Fs;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using TkSharp.Core;
using TkSharp.Core.IO;
using TkSharp.Data.Embedded;
using TkSharp.Extensions.LibHac;

namespace Tkmm.Core.Providers;

public class TkRomProvider : ITkRomProvider
{
    public ITkRom GetRom()
    {
        TkChecksums checksums = TkChecksums.FromStream(TkEmbeddedDataSource.GetChecksumsBin());
        TkConfig tk = TkConfig.Shared;

        if (tk.GameDumpFolderPath is string gameDumpFolderPath && TkRomHelper.IsRomfsValid(gameDumpFolderPath)) {
            return new ExtractedTkRom(gameDumpFolderPath, checksums);
        }

        if (TkConfig.Shared.KeysFolderPath is not string keysFolderPath || TkRomHelper.GetKeys(keysFolderPath) is not KeySet keys) {
            throw new InvalidOperationException("Invalid keys folder.");
        }

        if (TkConfig.Shared.BaseGameFilePath is not string baseGameFilePath ||
            !TkRomHelper.IsTotkRomFile(baseGameFilePath, keys, out Application? app) || app.DisplayVersion != "1.0.0") {
            throw new InvalidOperationException("Invalid base game file.");
        }
        
        if (TkConfig.Shared.GameUpdateFilePath is not string gameUpdateFilePath ||
            !TkRomHelper.IsTotkRomFile(gameUpdateFilePath, keys, out Application? update) || update.DisplayVersion == "1.0.0") {
            throw new InvalidOperationException("Invalid update file.");
        }
        
        return new PackedTkRom(checksums, keys, baseGameFilePath, gameUpdateFilePath);
    }

    /// <summary>
    /// Returns true if a rom can be provided with the current configurations. 
    /// </summary>
    public static bool CanProvideRom()
    {
        return CanProvideRom(message => TkLog.Instance.LogWarning("[Verify TotK Rom] {Message}", message));
    }

    public static bool CanProvideRom(Action<string> log)
    {
        if (string.IsNullOrWhiteSpace(TkConfig.Shared.GameDumpFolderPath)) {
            log("[RomFS Dump] The game dump folder path is empty.");
            goto UseNspXci;
        }

        if (TkRomHelper.GetVersionFromRomfs(TkConfig.Shared.GameDumpFolderPath) is not int version) {
            log("[RomFS Dump] Invalid game dump folder path.");
            goto UseNspXci;
        }

        if (version == 100) {
            log("[RomFS Dump] Invalid game dump version (1.0.0), TKMM requires version 1.1.0 or later.");
        }

        return true;

    UseNspXci:
        if (TkConfig.Shared.KeysFolderPath is not string keysFolderPath || TkRomHelper.GetKeys(keysFolderPath) is not KeySet keys) {
            log("[XCI/NSP] The keys folder is invalid.");
            return false;
        }

        if (TkConfig.Shared.BaseGameFilePath is not string baseGameFilePath ||
            !TkRomHelper.IsTotkRomFile(baseGameFilePath, keys, out Application? app) || app.DisplayVersion != "1.0.0") {
            log("[XCI/NSP] The base game file is invalid.");
            return false;
        }

        if (TkConfig.Shared.GameUpdateFilePath is not string gameUpdateFilePath ||
            !TkRomHelper.IsTotkRomFile(gameUpdateFilePath, keys, out Application? update) || update.DisplayVersion == "1.0.0") {
            log("[XCI/NSP] The game update file is invalid.");
            return false;
        }

        return true;
    }
}