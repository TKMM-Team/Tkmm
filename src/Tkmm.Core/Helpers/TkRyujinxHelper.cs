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

public class TkRyujinxHelper
{
    /// <summary>
    /// Attempts to use the running Ryujinx instance to configure <see cref="TkConfig"/>.
    /// </summary>
    /// <param name="hasUpdate"></param>
    /// <returns></returns>
    public static Either<bool, string> UseRyujinx(out bool hasUpdate)
    {
        bool result = false;
        hasUpdate = false;
        
        if (GetRyujinxDataFolder(out string? ryujinxExeFilePath) is not string ryujinxDataFolder) {
            return Locale["RyujinxDataFolderNotFound"];
        }

        Config.Shared.EmulatorPath = ryujinxExeFilePath;

        if (GetRyujinxConfig(ryujinxDataFolder) is not RyujinxConfig config) {
            return Locale["RyujinxConfigNotFound"];
        }

        if (GetRyujinxKeys(ryujinxDataFolder, out string systemFolderPath) is not KeySet keys) {
            return Locale["RyujinxKeysNotFound"];
        }
        
        TkConfig.Shared.KeysFolderPath = systemFolderPath;
        
        string modFolderPath = Path.Combine(ryujinxDataFolder, "mods", "contents", "0100f2c0115b6000", "TKMM");
        
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
        string path = Path.Combine(ryujinxDataFolder, "Config.json");
        
        if (!File.Exists(path)) {
            TkLog.Instance.LogError("Ryujinx config file could not be found.");
            return null;
        }
        
        using FileStream fs = File.OpenRead(path);
        return JsonSerializer.Deserialize<RyujinxConfig>(fs);
    }

    private static string? GetRyujinxDataFolder(out string? ryujinxExeFilePath)
    {
        Process? ryujinx = Process
            .GetProcesses()
            .FirstOrDefault(x => x.ProcessName.Contains("Ryujinx", StringComparison.OrdinalIgnoreCase));

        ryujinxExeFilePath = ryujinx?.MainModule?.FileName;

        if (ryujinxExeFilePath is null) {
            return null;
        }

        if (Path.GetDirectoryName(ryujinxExeFilePath) is not string ryujinxFolder) {
            goto UseAppDataInstall;
        }
        
        string portable = Path.Combine(ryujinxFolder, "portable");
        
        if (Directory.Exists(portable)) {
            return portable;
        }
        
    UseAppDataInstall:
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx");
    }

    public static string? GetSelectedUpdatePath()
    {
        if (GetRyujinxDataFolder(out string? dummy) is not string ryujinxDataFolder)
        {
            return null;
        }

        string updatesJsonPath = Path.Combine(ryujinxDataFolder, "games", "0100f2c0115b6000", "updates.json");
        if (File.Exists(updatesJsonPath))
        {
            try
            {
                string json = File.ReadAllText(updatesJsonPath);
                var updates = JsonSerializer.Deserialize<RyujinxUpdates>(json);
                if (updates != null && !string.IsNullOrWhiteSpace(updates.Selected))
                {
                    return updates.Selected;
                }
            }
            catch (Exception ex)
            {
                TkLog.Instance.LogError("Failed to read updates.json: {Error}", ex.Message);
            }
        }
        return null;
    }
}

public record RyujinxConfig(
    [property: JsonPropertyName("game_dirs")] List<string> GameDirs
);

public record RyujinxUpdates(
    [property: JsonPropertyName("selected")] string Selected
);

#endif
