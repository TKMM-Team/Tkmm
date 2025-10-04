using ConsoleAppFramework;
using System.Collections.Concurrent;

namespace Tkmm.CLI;

public static class TkConsoleApp
{
    private static readonly ConcurrentQueue<long> PendingModViewerRequests = new();

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
    /// <param name="args"></param>
    /// <param name="install"></param>
    /// <returns></returns>
    public static void ProcessBasicArgs(string[] args, Func<string, Stream?, Task> install)
    {
        foreach (string arg in args.Select(x => x.Trim('"')).Where(x => Path.Exists(x) || (x.Length > 5 && x.AsSpan()[..5] is "tkmm:"))) {
            if (arg.StartsWith("tkmm://mod/")) {
                var modId = arg.Substring("tkmm://mod/".Length);
                if (long.TryParse(modId, out var id)) {
                    PendingModViewerRequests.Enqueue(id);
                }
            }
            else {
                _ = Task.Run(async () => await install(arg, File.Exists(arg) ? File.OpenRead(arg) : null));
            }
        }
    }

    public static void StartCli(string[] args)
    {
        ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();
        app.Run(args);
    }

    public static bool TryGetPendingModViewerRequest(out long modId)
    {
        return PendingModViewerRequests.TryDequeue(out modId);
    }
}