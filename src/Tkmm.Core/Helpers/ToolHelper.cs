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
        Trace.WriteLine($"[Debug] {absoluePath}");
        Trace.WriteLine($"[Debug] \"{string.Join("\" \"", args)}\"");
        return Process.Start(absoluePath, args);
    }
}
