using System.Diagnostics;

namespace Tkmm.Core.Helpers;

/// <summary>
/// Helper class to call CLI tools
/// </summary>
public class ToolHelper
{
    private static readonly Dictionary<string, string> _tools = new() {
        { "SarcTool", "TKMM.SarcTool.exe" },
        { "RsdbMerge", "rsdb-merge.exe" },
        { "MalsMerger", "MalsMerger.exe" },
        { "Restbl", "restbl.exe" },
    };

    public static Process Call(string toolName, params string[] args)
    {
        if (!_tools.TryGetValue(toolName, out var tool)) {
            throw new KeyNotFoundException($"""
                The tool {toolName} could not be found!
                """);
        }

        string absoluePath = Path.Combine(Config.Shared.StaticStorageFolder, tool);
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
