#if !SWITCH

using LanguageExt;
using LibHac.Common.Keys;
using LibHac.Tools.Fs;
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
    
    public static Either<bool, string> UseEmulator(string emulatorFilePath, out bool hasUpdate)
    {
        bool result = false;
        hasUpdate = false;

        if (!File.Exists(emulatorFilePath)) {
            return Locale["EmulatorFilePathNotFound", emulatorFilePath];
        }

        Config.Shared.EmulatorPath = emulatorFilePath;

        if (TryGetEmulatorDataFolder(emulatorFilePath, out string emulatorDataFolderPath, out string emulatorName)
            is not string emulatorConfigFilePath) {
            return Locale["EmulatorConfigFileNotFound", emulatorName];
        }

        if (GetKeys(emulatorDataFolderPath, out string keysFolderPath) is not KeySet keys) {
            return Locale["EmulatorKeysNotFound", emulatorName];
        }

        TkConfig.Shared.KeysFolderPath = keysFolderPath;

        string nandFolderPath = Path.Combine(emulatorDataFolderPath, "nand");
        TkConfig.Shared.NandFolderPaths.New(nandFolderPath);

        string modFolderPath = GetModFolder(emulatorDataFolderPath);

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
        foreach ((Application totk, string filePath) in TkGameRomUtils.ScanFolders(configuredGamePaths, keys)) {
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
            : Path.GetFileName(emulatorFilePath);

        if (Path.GetDirectoryName(emulatorFilePath) is string exeFolder
            && Path.Combine(exeFolder, "user") is var portableDataFolderPath
            && Directory.Exists(portableDataFolderPath)) {
            emulatorDataFolderPath = portableDataFolderPath;
            if (CheckConfig(emulatorDataFolderPath) is var result) {
                return result;
            }
        }

        emulatorDataFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), emulatorName);

        string emulatorConfigFilePath = Path.Combine(emulatorDataFolderPath, "config", "qt-config.ini");
        return File.Exists(emulatorConfigFilePath) ? emulatorConfigFilePath : null;
    }

    private static string? CheckConfig(string emulatorDataFolderPath)
    {
        string emulatorConfigFilePath = Path.Combine(emulatorDataFolderPath, "config", "qt-config.ini");
        return File.Exists(emulatorConfigFilePath) ? emulatorConfigFilePath : null;
    }

    private static KeySet? GetKeys(string emulatorDataFolder, out string keysFolderPath)
    {
        keysFolderPath = Path.Combine(emulatorDataFolder, "keys");
        return TkKeyUtils.GetKeysFromFolder(keysFolderPath);
    }

    private static List<string> GetGameFolderPaths(string qtConfigFilePath)
    {
        using FileStream fs = File.OpenRead(qtConfigFilePath);
        using StreamReader reader = new(fs);

        List<string> results = [];

        Span<Range> ranges = stackalloc Range[2];
        ref Range itemKey = ref ranges[0];
        ref Range itemValue = ref ranges[1];

        while (reader.ReadLine() is string line) {
            ReadOnlySpan<char> item = line.AsSpan();

            if (item.Length < 24 || item[..15] is not @"Paths\gamedirs\") {
                continue;
            }

            if (item.Split(ranges, '=') < 2 || item[itemKey][^4..] is not "path") {
                continue;
            }

            string romFolderPath = line[itemValue];

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