using ConsoleAppFramework;

namespace Tkmm.CLI;

public static class TkConsoleApp
{
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
            _ = Task.Run(async () => await install(arg, File.Exists(arg) ? File.OpenRead(arg) : null));
        }
    }

    public static void StartCli(string[] args)
    {
        ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();
        app.Run(args);
    }
}