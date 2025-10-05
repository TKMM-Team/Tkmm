using ConsoleAppFramework;
using System.Collections.Concurrent;

namespace Tkmm.CLI;

public static class TkConsoleApp
{
    public static event Func<string, Stream?, Task>? InstallRequested;
    public static event Func<long, Task>? OpenModRequested;

    /// <summary>
    /// Checks if the arguments form a complex request.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static bool IsComplexRequest(string[] args)
    {
        return !args.All(x => Path.Exists(x) || (x.Length > 5 && x.AsSpan()[..5] is "tkmm:"));
    }

    /// <summary>
    /// Processes basic input arguments and returns the number of complex arguments. 
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    public static void ProcessBasicArgs(string[] args)
    {
        foreach (var raw in args) {
            var arg = raw.Trim('"');
            if (!(Path.Exists(arg) || (arg.Length > 5 && arg.AsSpan()[..5] is "tkmm:"))) {
                continue;
            }

            if (arg.StartsWith("tkmm://mod/")) {
                var modIdStr = arg.Substring("tkmm://mod/".Length);
                if (!long.TryParse(modIdStr, out var id)) {
                    continue;
                }
                if (OpenModRequested is not null) {
                    _ = OpenModRequested.Invoke(id);
                }
            }
            else {
                Stream? stream = null;
                if (File.Exists(arg)) {
                    stream = File.OpenRead(arg);
                }
                if (InstallRequested is not null) {
                    _ = InstallRequested.Invoke(arg, stream);
                }
            }
        }
    }

    public static void StartCli(string[] args)
    {
        var app = ConsoleApp.Create();
        ProcessBasicArgs(args);
        app.Run(args);
    }
}