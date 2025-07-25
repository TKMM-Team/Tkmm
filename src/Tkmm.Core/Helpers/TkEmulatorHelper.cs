#if !SWITCH

using LanguageExt;
using LibHac.Common.Keys;
using Tkmm.Core.Models;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Core.Helpers;

public static class TkEmulatorHelper
{
    public static string? GetModPath(string emulatorFilePath)
    {
        ReadOnlySpan<char> exePath = emulatorFilePath;

        if (Path.GetFileNameWithoutExtension(exePath).Equals("ryujinx", StringComparison.InvariantCultureIgnoreCase)) {
            return TkRyujinxHelper.GetModPath(emulatorFilePath);
        }
        
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (TryGetEmulatorDataFolder(emulatorFilePath, out string emulatorDataFolderPath, out _) is null) {
            return null;
        }
        
        return GetModFolder(emulatorDataFolderPath);
    }

    public static string? GetSdPath(string emulatorFilePath)
    {
        ReadOnlySpan<char> exePath = emulatorFilePath;

        if (Path.GetFileNameWithoutExtension(exePath).Equals("ryujinx", StringComparison.InvariantCultureIgnoreCase)) {
            return TkRyujinxHelper.GetSdPath(emulatorFilePath);
        }
        
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (TryGetEmulatorDataFolder(emulatorFilePath, out string emulatorDataFolderPath, out _) is null) {
            return null;
        }
        
        return Path.Combine(emulatorDataFolderPath, "sdmc");
    }
    
    public static string? GetNandPath(string emulatorFilePath)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (TryGetEmulatorDataFolder(emulatorFilePath, out string emulatorDataFolderPath, out _) is null) {
            return null;
        }
        
        return Path.Combine(emulatorDataFolderPath, "nand");
    }
    
    public static Either<bool, string> UseEmulator(string emulatorFilePath, out bool hasUpdate)
    {
        bool result = false;
        hasUpdate = false;

        // Only check file existence if the path contains directory separators
        if (emulatorFilePath.Contains(Path.DirectorySeparatorChar) || emulatorFilePath.Contains(Path.AltDirectorySeparatorChar)) {
            if (!File.Exists(emulatorFilePath)) {
                return Locale["EmulatorFilePathNotFound", emulatorFilePath];
            }
        }

        Config.Shared.EmulatorPath = emulatorFilePath;

        if (TryGetEmulatorDataFolder(emulatorFilePath, out var emulatorDataFolderPath, out var emulatorName)
            is not { } emulatorConfigFilePath) {
            return Locale["EmulatorConfigFileNotFound", emulatorName];
        }

        if (GetKeys(emulatorDataFolderPath, out var keysFolderPath) is not { } keys) {
            return Locale["EmulatorKeysNotFound", emulatorName];
        }

        TkConfig.Shared.KeysFolderPath = keysFolderPath;

        var nandFolderPath = Path.Combine(emulatorDataFolderPath, "nand");
        TkConfig.Shared.NandFolderPaths.New(nandFolderPath);

        var modFolderPath = GetModFolder(emulatorDataFolderPath);

        if (string.IsNullOrWhiteSpace(Config.Shared.MergeOutput)) {
            Directory.CreateDirectory(modFolderPath);
            Config.Shared.MergeOutput = modFolderPath;
        }
        else {
            Config.Shared.ExportLocations.Add(new ExportLocation {
                SymlinkPath = modFolderPath,
                IsEnabled = true
            });
        }

        if (GetGameFolderPaths(emulatorConfigFilePath) is not { Count: > 0 } gameFolderPaths) {
            return false;
        }

        CheckConfiguredGamePaths(ref result, ref hasUpdate, gameFolderPaths, keys);

        return result;
    }

    public static bool CheckConfiguredGamePaths(ref bool result, ref bool hasUpdate, IEnumerable<string> configuredGamePaths, KeySet keys)
    {
        foreach (var (totk, filePath) in TkGameRomUtils.ScanFolders(configuredGamePaths, keys)) {
            if (totk.Main is not null) {
                result = true;
                TkConfig.Shared.PackagedBaseGamePaths.New(filePath);
            }

            if (totk.Patch is not null && totk.DisplayVersion is not "100") {
                hasUpdate = true;
                TkConfig.Shared.PackagedUpdatePaths.New(filePath);
            }
        }

        return result;
    }

    private static string? TryGetEmulatorDataFolder(string emulatorFilePath, out string emulatorDataFolderPath, out string emulatorName)
    {
        emulatorName = OperatingSystem.IsWindows()
                ? Path.GetFileNameWithoutExtension(emulatorFilePath)
                : emulatorFilePath.EndsWith(".AppImage", StringComparison.OrdinalIgnoreCase)
                    ? Path.GetFileNameWithoutExtension(emulatorFilePath).ToLower()
                    : Path.GetFileName(emulatorFilePath).ToLower();

        if (Path.GetDirectoryName(emulatorFilePath) is { } exeFolder
            && Path.Combine(exeFolder, "user") is var portableDataFolderPath
            && Directory.Exists(portableDataFolderPath)) {
            emulatorDataFolderPath = portableDataFolderPath;
            if (CheckConfig(emulatorDataFolderPath) is var result) {
                return result;
            }
        }

        if (OperatingSystem.IsWindows()) {
            emulatorDataFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), emulatorName);
    
            var emulatorConfigFilePath = Path.Combine(emulatorDataFolderPath, "config", "qt-config.ini");
            return File.Exists(emulatorConfigFilePath) ? emulatorConfigFilePath : null;
        }
        else
        {
            var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
                                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
            var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME")
                              ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");

            var configPath = Path.Combine(xdgConfigHome, emulatorName);
            emulatorDataFolderPath = Path.Combine(xdgDataHome, emulatorName);

            var emulatorConfigFilePath = Path.Combine(configPath, "qt-config.ini");
            return File.Exists(emulatorConfigFilePath) ? emulatorConfigFilePath : null;
        }
    }

    private static string? CheckConfig(string emulatorDataFolderPath)
    {
        var emulatorConfigFilePath = OperatingSystem.IsWindows()
            ? Path.Combine(emulatorDataFolderPath, "config", "qt-config.ini")
            : Path.Combine(emulatorDataFolderPath, "qt-config.ini");
        return File.Exists(emulatorConfigFilePath) ? emulatorConfigFilePath : null;
    }

    private static KeySet? GetKeys(string emulatorDataFolder, out string keysFolderPath)
    {
        keysFolderPath = Path.Combine(emulatorDataFolder, "keys");
        return TkKeyUtils.GetKeysFromFolder(keysFolderPath);
    }

    private static List<string> GetGameFolderPaths(string qtConfigFilePath)
    {
        using var fs = File.OpenRead(qtConfigFilePath);
        using StreamReader reader = new(fs);

        List<string> results = [];

        Span<Range> ranges = stackalloc Range[2];
        ref var itemKey = ref ranges[0];
        ref var itemValue = ref ranges[1];

        while (reader.ReadLine() is { } line) {
            var item = line.AsSpan();

            if (item.Length < 24 || item[..15] is not @"Paths\gamedirs\") {
                continue;
            }

            if (item.Split(ranges, '=') < 2 || item[itemKey][^4..] is not "path") {
                continue;
            }

            var romFolderPath = line[itemValue];

            if (!Directory.Exists(romFolderPath)) {
                continue;
            }

            results.Add(romFolderPath);
        }

        return results;
    }
    
    private static string GetModFolder(string emulatorDataFolderPath)
        => Path.Combine(emulatorDataFolderPath, "load", "0100F2C0115B6000", "TKMM");
}

#endif