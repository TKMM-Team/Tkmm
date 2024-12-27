using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LibHac.Common.Keys;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using Microsoft.Extensions.Logging;
using TkSharp.Core;
using TkSharp.Extensions.LibHac;
using TkSharp.Extensions.LibHac.Extensions;

namespace Tkmm.Helpers;

// TODO: Translate error messages

public class RyujinxHelper
{
    public static KeySet? GetKeys(out string systemFolderPath)
    {
        systemFolderPath = Path.Combine(GetRyujinxDataFolder(), "system");
        
        string prodKeysFilePath = Path.Combine(systemFolderPath, "prod.keys");
        if (!File.Exists(prodKeysFilePath)) {
            TkLog.Instance.LogError("A 'prod.keys' file could not be found in '{RyujinxSystemFolder}'", systemFolderPath);
            return null;
        }
        
        string titleKeysFilePath = Path.Combine(systemFolderPath, "title.keys");
        if (!File.Exists(prodKeysFilePath)) {
            TkLog.Instance.LogError("A 'title.keys' file could not be found in '{RyujinxSystemFolder}'", systemFolderPath);
            return null;
        }

        KeySet keys = new();
        ExternalKeyReader.ReadKeyFile(keys,
            prodKeysFilename: prodKeysFilePath,
            titleKeysFilename: titleKeysFilePath);

        return keys;
    }
    
    public static IEnumerable<(string FilePath, string Version)> GetTotkFiles(KeySet keys)
    {
        RyujinxConfig? config = GetRyujinxConfig();

        if (config is null) {
            throw new InvalidOperationException("Ryujinx configuration could not be found.");
        }

        foreach (string target in config.GameDirs.SelectMany(dir => Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))) {
            ReadOnlySpan<char> extension = Path.GetExtension(target.AsSpan());
            if (extension is not (".nsp" or ".xci")) {
                continue;
            }

            using LocalStorage storage = new(target, FileAccess.Read);
            using SwitchFs fs = storage.GetSwitchFs(target, keys);

            if (fs.Applications.TryGetValue(PackedTkRom.EX_KING_APP_ID, out Application? app)) {
                yield return (FilePath: target, app.DisplayVersion);
            }
        }
    }

    public static RyujinxConfig? GetRyujinxConfig()
    {
        string path = Path.Combine(GetRyujinxDataFolder(), "Config.json");
        
        if (!File.Exists(path)) {
            TkLog.Instance.LogError("Ryujinx config file could not be found.");
            return null;
        }
        
        using FileStream fs = File.OpenRead(path);
        return JsonSerializer.Deserialize<RyujinxConfig>(fs);
    }

    public static string GetRyujinxDataFolder(bool failIfNotRunning = false)
    {
        Process? ryujinx = Process
            .GetProcesses()
            .FirstOrDefault(x => x.ProcessName.Contains("Ryujinx", StringComparison.OrdinalIgnoreCase));

        if (ryujinx is null) {
            if (failIfNotRunning) throw new InvalidOperationException("Ryujinx is not running.");
            
            TkLog.Instance.LogError("Ryujinx is not running, defaulting to AppData installation.");
            goto UseAppDataInstall;
        }

        if (Path.GetDirectoryName(ryujinx?.MainModule?.FileName) is not string ryujinxFolder) {
            goto UseAppDataInstall;
        }
        
        string portable = Path.Combine(ryujinxFolder, "portable");
        
        if (Directory.Exists(portable)) {
            return portable;
        }
        
    UseAppDataInstall:
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx");
    }
}

public record RyujinxConfig(
    [property: JsonPropertyName("game_dirs")] List<string> GameDirs
);
