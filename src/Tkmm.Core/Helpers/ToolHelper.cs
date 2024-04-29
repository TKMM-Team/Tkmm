using Humanizer;
using System.Diagnostics;
using System.Text.Json;
using Tkmm.Core.Helpers.Models;
using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Helpers;

public enum Tool
{
    SarcTool = 0,
    RsdbMerger = 1
}

/// <summary>
/// Helper class to call CLI tools
/// </summary>
public class ToolHelper
{
    private static readonly string _depsPath = Path.Combine(Config.Shared.StaticStorageFolder, "deps.json");
    private static readonly string _appsDir = Path.Combine(Config.Shared.StaticStorageFolder, "apps");

    public static Dictionary<Tool, Dependency> Deps { get; private set; } = [];
    public static List<string> ExcludeFolders { get; private set; } = [];
    public static List<string> ExcludeFiles { get; private set; } = [];

    public static async Task LoadDeps(bool forceRefresh = false)
    {
        if (File.Exists(_depsPath) && !forceRefresh) {
            using FileStream fs = File.OpenRead(_depsPath);
            Deps = JsonSerializer.Deserialize<Dictionary<Tool, Dependency>>(fs)
                ?? throw new InvalidOperationException("""
                    Could not parse deps, the JsonDeserializer returned null
                    """);

            goto FillExcude;
        }

        byte[] data = await GitHubOperations.GetAsset("TKMM-Team", ".github", "deps.json");
        Deps = JsonSerializer.Deserialize<Dictionary<Tool, Dependency>>(data)
            ?? throw new InvalidOperationException("""
                Could not parse deps, the JsonDeserializer returned null
                """);

        using (FileStream writer = File.Create(_depsPath)) {
            writer.Write(data);
        }

    FillExcude:
        var exclude = Deps
            .Select(x => x.Value.Exclude)
            .Aggregate<IEnumerable<string>>((x, y) => x.Concat(y))
            .ToArray();

        ExcludeFolders = exclude
            .Where(x => x.StartsWith('/'))
            .Select(x => x[1..])
            .ToList();

        ExcludeFiles = exclude
            .Where(x => x.StartsWith('.'))
            .ToList();
    }

    public static Process Call(Tool tool, params string[] args)
    {
        if (!Deps.TryGetValue(tool, out var dependency)) {
            throw new KeyNotFoundException($"""
                The tool {tool} could not be found!
                """);
        }

        string absoluePath = Path.Combine(_appsDir, dependency.Repo, dependency.Files[Dependency.GetOSName()]);
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

    public static async Task DownloadDependencies(Action<double>? updateProgress = null, bool forceRefresh = false)
    {
        AppStatus.Set("Downloading dependencies", "fa-solid fa-download", isWorkingStatus: true);

        if (Deps.Count <= 0) {
            await LoadDeps(forceRefresh);
        }

        double inc = 70 / Deps.Count;

        List<Task> tasks = [];

        Directory.CreateDirectory(_appsDir);
        foreach ((_, var dep) in Deps) {
            tasks.Add(Task.Run(async () => {
                AppStatus.Set($"Downloading '{dep.Owner}/{dep.Repo}'", "fa-solid fa-download", isWorkingStatus: true);
                await dep.Download();
                updateProgress?.Invoke(inc);
            }));
        }

        await Task.WhenAll(tasks);
        AppStatus.Set("Dependencies restored!", "fa-solid fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
    }
}
