using System.Diagnostics;
using System.Text.Json;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models.Tools;

namespace Tkmm.Core.Helpers;

public enum Tool
{
    MalsMerger = 0,
    SarcTool = 1,
    RsdbMerger = 2,
    RestblMerger = 3
}

/// <summary>
/// Helper class to call CLI tools
/// </summary>
public class ToolHelper
{
    private static readonly string _depsPath = Path.Combine(Config.Shared.StaticStorageFolder, "deps.json");

    public static Dictionary<Tool, Dependency> Deps { get; private set; } = [];

    public static async Task LoadDeps()
    {
        if (File.Exists(_depsPath)) {
            using FileStream fs = File.OpenRead(_depsPath);
            Deps = JsonSerializer.Deserialize<Dictionary<Tool, Dependency>>(fs)
                ?? throw new InvalidOperationException("""
                    Could not parse deps, the JsonDeserializer returned null
                    """);

            return;
        }

        byte[] data = await GitHubOperations.GetAsset("TKMM-Team", ".github", "deps.json");
        Deps = JsonSerializer.Deserialize<Dictionary<Tool, Dependency>>(data)
            ?? throw new InvalidOperationException("""
                Could not parse deps, the JsonDeserializer returned null
                """);

        using FileStream writer = File.Create(_depsPath);
        writer.Write(data);
    }

    public static Process Call(Tool tool, params string[] args)
    {
        if (!Deps.TryGetValue(tool, out var dependency)) {
            throw new KeyNotFoundException($"""
                The tool {tool} could not be found!
                """);
        }

        string absoluePath = Path.Combine(Config.Shared.StaticStorageFolder, dependency.Files[Dependency.GetOSName()]);
        AppLog.Log(absoluePath, LogLevel.Debug);
        AppLog.Log($"\"{string.Join("\" \"", args)}\"", LogLevel.Debug);

        Process proc = new() {
            StartInfo = new(absoluePath, args) {
                RedirectStandardOutput = true
            }
        };

        proc.OutputDataReceived += (s, e) => {
            if (e.Data is string msg) {
                AppLog.Log(msg, LogLevel.Default);
            }
        };

        proc.Start();
        proc.BeginOutputReadLine();
        return proc;
    }
}
