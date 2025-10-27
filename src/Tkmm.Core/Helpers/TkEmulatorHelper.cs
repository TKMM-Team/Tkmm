#if !SWITCH

using System.Diagnostics;
using LanguageExt;
using LibHac.Common.Keys;
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
        if (TryGetEmulatorDataFolder(emulatorFilePath, out var emulatorDataFolderPath, out _) is null) {
            return null;
        }
        
        return GetModFolder(GetEmulatorConfigPath(emulatorDataFolderPath), emulatorFilePath);
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
        
        return GetDirectoryFromConfig(GetEmulatorConfigPath(emulatorDataFolderPath), "sdmc_directory");
    }
    
    public static string? GetNandPath(string emulatorFilePath)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (TryGetEmulatorDataFolder(emulatorFilePath, out string emulatorDataFolderPath, out _) is null) {
            return null;
        }
        
        return GetDirectoryFromConfig(GetEmulatorConfigPath(emulatorDataFolderPath), "nand_directory");
    }
    
    public static Either<bool, string> UseEmulator(string emulatorFilePath, out bool hasUpdate)
    {
        var result = false;
        hasUpdate = false;

        // Only check file existence if the path contains directory separators
        if (emulatorFilePath.Contains(Path.DirectorySeparatorChar) || emulatorFilePath.Contains(Path.AltDirectorySeparatorChar)) {
            if (!File.Exists(emulatorFilePath)) {
                return Locale["EmulatorFilePathNotFound", emulatorFilePath];
            }
        }
        else {
            // if only the emulator name was provided, attempt to find it from running processes
            var process = Process.GetProcesses()
                .FirstOrDefault(x => x.ProcessName.Equals(emulatorFilePath, StringComparison.OrdinalIgnoreCase));
            
            if (process?.MainModule?.FileName is { } fullPath) {
                emulatorFilePath = fullPath;
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

        var nandFolderPath = GetDirectoryFromConfig(emulatorConfigFilePath, "nand_directory");
        
        if (Directory.Exists(nandFolderPath)) {
            TkConfig.Shared.NandFolderPaths.New(nandFolderPath);
        }

        if (GetModFolder(emulatorConfigFilePath, emulatorFilePath) is {} modFolderPath) {
            Config.Shared.MergeOutput = modFolderPath;
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
            var configPath = GetEmulatorConfigPath(emulatorDataFolderPath);
            if (File.Exists(configPath)) {
                return configPath;
            }
        }

        if (OperatingSystem.IsWindows()) {
            emulatorDataFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), emulatorName);
    
            var emulatorConfigFilePath = GetConfigFilePath(emulatorName, emulatorDataFolderPath);
            return File.Exists(emulatorConfigFilePath) ? emulatorConfigFilePath : null;
        }
        else {
            var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME")
                              ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");

            emulatorDataFolderPath = Path.Combine(xdgDataHome, emulatorName);
            var emulatorConfigFilePath = GetConfigFilePath(emulatorName, emulatorDataFolderPath);
            return File.Exists(emulatorConfigFilePath) ? emulatorConfigFilePath : null;
        }
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
    
    private static string GetEmulatorConfigPath(string emulatorDataFolderPath) {
        var emulatorName = Path.GetFileName(emulatorDataFolderPath);
        return GetConfigFilePath(emulatorName, emulatorDataFolderPath);
    }
    
    private static string? GetModFolder(string emulatorConfigFilePath, string emulatorFilePath)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Config.Shared is null) {
            return null;
        }
        
        if (Config.Shared.UseRomfslite && GetSdPath(emulatorFilePath) is {} sdCardPath) {
            return Path.Combine(sdCardPath, "atmosphere", "contents", "0100F2C0115B6000").Replace('\\', '/');
        }
        
        return GetDirectoryFromConfig(emulatorConfigFilePath, "load_directory") is not { } loadDir 
            ? "" : Path.Combine(loadDir, "0100F2C0115B6000", "TKMM").Replace('\\', '/');
    }

    private static string? GetDirectoryFromConfig(string emulatorConfigFilePath, string configKey) {
        if (!File.Exists(emulatorConfigFilePath)) {
            return null;
        }

        using var fs = File.OpenRead(emulatorConfigFilePath);
        using StreamReader reader = new(fs);

        while (reader.ReadLine() is { } line) {
            var trimmedLine = line.Trim();

            if (!trimmedLine.StartsWith($"{configKey}=", StringComparison.OrdinalIgnoreCase)) {
                continue;
            }
            
            var directoryPath = trimmedLine[$"{configKey}=".Length..].Trim();
            if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath)) {
                return directoryPath;
            }
        }

        return null;
    }

    private static string GetConfigFilePath(string emulatorName, string emulatorDataFolderPath)
    {
        if (OperatingSystem.IsWindows()) {
            return Path.Combine(emulatorDataFolderPath, "config", "qt-config.ini");
        }
        else {
            var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
                                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
            return Path.Combine(xdgConfigHome, emulatorName, "qt-config.ini");
        }
    }
}

#endif