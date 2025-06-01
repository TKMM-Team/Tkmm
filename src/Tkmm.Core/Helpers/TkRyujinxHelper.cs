#if !SWITCH

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using LibHac.Common.Keys;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Models;
using TkSharp.Core;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Core.Helpers;

public static class TkRyujinxHelper
{
    /// <summary>
    /// Attempts to use the running Ryujinx instance to configure <see cref="TkConfig"/>.
    /// </summary>
    /// <param name="hasUpdate"></param>
    /// <param name="manualSetup"></param>
    /// <returns></returns>
    public static Either<bool, string> UseRyujinx(out bool hasUpdate, bool manualSetup = false)
    {
        var result = false;
        hasUpdate = false;


        if (GetRyujinxDataFolderFromProcess(out string? ryujinxExeFilePath) is not { } dataFolder) {
            if (manualSetup) {
                if (GetRyujinxDataFolder("ryujinx") is not { } manualDataFolder) {
                    return Locale["RyujinxDataFolderNotFound"];
                }
                dataFolder = manualDataFolder;
                ryujinxExeFilePath = "ryujinx";
            }
            else {
                return Locale["RyujinxDataFolderNotFound"];
            }
        }
        
        var ryujinxDataFolder = dataFolder;
        Config.Shared.EmulatorPath = ryujinxExeFilePath;
        
        if (GetRyujinxConfig(ryujinxDataFolder) is not { } config) {
            return Locale["RyujinxConfigNotFound"];
        }

        if (GetRyujinxKeys(ryujinxDataFolder, out string systemFolderPath) is not { } keys) {
            return Locale["RyujinxKeysNotFound"];
        }

        TkConfig.Shared.KeysFolderPath = systemFolderPath;

        string modFolderPath = GetModFolder(ryujinxDataFolder);

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

        return TkEmulatorHelper.CheckConfiguredGamePaths(ref result, ref hasUpdate, config.GameDirs, keys);
    }

    private static KeySet? GetRyujinxKeys(string ryujinxDataFolder, out string systemFolderPath)
    {
        systemFolderPath = Path.Combine(ryujinxDataFolder, "system");
        return TkKeyUtils.GetKeysFromFolder(systemFolderPath);
    }

    private static RyujinxConfig? GetRyujinxConfig(string ryujinxDataFolder)
    {
        var path = Path.Combine(ryujinxDataFolder, "Config.json");

        if (!File.Exists(path)) {
            TkLog.Instance.LogError("Ryujinx config file could not be found.");
            return null;
        }

        using FileStream fs = File.OpenRead(path);
        return JsonSerializer.Deserialize<RyujinxConfig>(fs);
    }

    private static string? GetRyujinxDataFolderFromProcess(out string? ryujinxExeFilePath)
    {
        Process? ryujinx = Process
            .GetProcesses()
            .FirstOrDefault(x => x.ProcessName.Contains("Ryujinx", StringComparison.OrdinalIgnoreCase));

        return (ryujinxExeFilePath = ryujinx?.MainModule?.FileName) is null
            ? null
            : GetRyujinxDataFolder(ryujinxExeFilePath);
    }

    private static string GetRyujinxDataFolder(string ryujinxExeFilePath)
    {
        if (Path.GetDirectoryName(ryujinxExeFilePath) is not { } ryujinxFolder) {
            goto UseAppDataInstall;
        }

        string portable = Path.Combine(ryujinxFolder, "portable");

        if (Directory.Exists(portable)) {
            return portable;
        }

    UseAppDataInstall:
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx");
    }

    public static string? GetSelectedUpdatePath(string emulatorFilePath)
    {
        string updatesJsonPath = Path.Combine(GetRyujinxDataFolder(emulatorFilePath), "games", "0100f2c0115b6000", "updates.json");
        if (File.Exists(updatesJsonPath)) {
            try {
                string json = File.ReadAllText(updatesJsonPath);
                var updates = JsonSerializer.Deserialize<RyujinxUpdates>(json);
                if (updates != null && !string.IsNullOrWhiteSpace(updates.Selected)) {
                    return updates.Selected;
                }
            }
            catch (Exception ex) {
                TkLog.Instance.LogError("Failed to read updates.json: {Error}", ex.Message);
            }
        }

        return null;
    }

    public static string GetModPath(string emulatorExecutablePath)
        => GetModFolder(GetRyujinxDataFolder(emulatorExecutablePath));

    public static string GetSdPath(string emulatorExecutablePath)
        => Path.Combine(GetRyujinxDataFolder(emulatorExecutablePath), "sdcard");

    private static string GetModFolder(string ryujinxDataFolder)
        => Path.Combine(ryujinxDataFolder, "mods", "contents", "0100f2c0115b6000", "TKMM");
}

public record RyujinxConfig(
    [property: JsonPropertyName("game_dirs")]
    List<string> GameDirs
);

public record RyujinxUpdates(
    [property: JsonPropertyName("selected")]
    string Selected
);

#endif