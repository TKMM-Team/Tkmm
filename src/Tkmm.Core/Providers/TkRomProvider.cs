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

        (LibHacRomSourceType? baseSource, string? basePath) = GetRomSource();
        if (baseSource is null || basePath is null) {
            throw new InvalidOperationException("Invalid configuration: Base game path is not set.");
        }

        if (baseSource == LibHacRomSourceType.File && 
            (TkConfig.Shared.BaseGameFilePath is not string baseGameFilePath ||
            !TkRomHelper.IsTotkRomFile(baseGameFilePath, keys, out Application? app) || app.DisplayVersion != "1.0.0")) {
            throw new InvalidOperationException("Invalid base game file.");
        }

        (LibHacRomSourceType? updateSource, string? updatePath) = GetUpdateSource();
        if (updateSource is null || updatePath is null) {
            throw new InvalidOperationException("Invalid configuration: Update path is not set.");
        }

        if (updateSource == LibHacRomSourceType.File && 
            (TkConfig.Shared.GameUpdateFilePath is not string gameUpdateFilePath ||
            !TkRomHelper.IsTotkRomFile(gameUpdateFilePath, keys, out Application? update) || update.DisplayVersion == "1.0.0")) {
            throw new InvalidOperationException("Invalid update file.");
        }

        var romProvider = new LibHacRomProvider();
        return romProvider.CreateRom(
            checksums,
            keys,
            baseSource.Value, basePath,
            updateSource.Value, updatePath);
    }

    private static (LibHacRomSourceType? Source, string? Path) GetRomSource()
    {
        TkConfig tk = TkConfig.Shared;

        if (tk.BaseGameFilePath is string path && File.Exists(path)) {
            return (LibHacRomSourceType.File, path);
        }
        
        if (tk.SplitFilesPath is string splitPath && Directory.Exists(splitPath)) {
            return (LibHacRomSourceType.SplitFiles, splitPath);
        }
        
        if (tk.SdCardRootPath is string sdPath && Directory.Exists(sdPath)) {
            return (LibHacRomSourceType.SdCard, sdPath);
        }
        
        return (null, null);
    }

    private static (LibHacRomSourceType? Source, string? Path) GetUpdateSource()
    {
        TkConfig tk = TkConfig.Shared;

        if (tk.GameUpdateFilePath is string path && File.Exists(path)) {
            return (LibHacRomSourceType.File, path);
        }

        if (tk.SdCardRootPath is string sdPath && Directory.Exists(sdPath)) {
            return (LibHacRomSourceType.SdCard, sdPath);
        }
        
        return (null, null);
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

        if (TkConfig.Shared.BaseGameFilePath is string baseGameFilePath && !string.IsNullOrWhiteSpace(baseGameFilePath) &&
            (!TkRomHelper.IsTotkRomFile(baseGameFilePath, keys, out Application? app) || app.DisplayVersion != "1.0.0")) {
            log("[XCI/NSP] The base game file is invalid.");
            return false;
        }

        if (TkConfig.Shared.GameUpdateFilePath is string gameUpdateFilePath && !string.IsNullOrWhiteSpace(gameUpdateFilePath) &&
            (!TkRomHelper.IsTotkRomFile(gameUpdateFilePath, keys, out Application? update) || update.DisplayVersion == "1.0.0")) {
            log("[XCI/NSP] The game update file is invalid.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(TkConfig.Shared.SdCardRootPath)) {
            log("[SD Card] The SD card path is not set.");
            return false;
        }

        string nintendoContentsPath = Path.Combine(TkConfig.Shared.SdCardRootPath, "Nintendo", "Contents");
        if (!Directory.Exists(nintendoContentsPath)) {
            log("[SD Card] The Nintendo/Contents folder is not present in the SD card path.");
            return false;
        }

        return true;
    }
}