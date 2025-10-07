using ConsoleAppFramework;

namespace Tkmm.CLI;

public static class TkConsoleApp
{
    public static event Func<string, Stream?, Task>? InstallRequested;
    public static event Func<long, long?, Task>? OpenModRequested;
    public static event Action<string>? PageRequested;
    public static event Action<string>? SettingsFocusRequested;

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

            // tkmm:// deep links
            if (arg.StartsWith("tkmm://", StringComparison.OrdinalIgnoreCase)) {
                if (TryHandleTkmmUri(arg)) {
                    continue;
                }
                throw new ArgumentException($"Invalid tkmm argument: {arg}");
            }

            // Local file/folder install
            if (Path.Exists(arg)) {
                Stream? stream = null;
                if (File.Exists(arg)) {
                    stream = File.OpenRead(arg);
                }
                if (InstallRequested is not null) {
                    _ = InstallRequested.Invoke(arg, stream);
                }
                continue;
            }

            throw new ArgumentException($"Invalid argument: {arg}");
        }
    }

    private static bool TryHandleTkmmUri(string uri)
    {
        var u = uri.TrimEnd('/');

        // Pages
        if (u.Equals("tkmm://home", StringComparison.OrdinalIgnoreCase)) { PageRequested?.Invoke("home"); return true; }
        if (u.Equals("tkmm://profiles", StringComparison.OrdinalIgnoreCase)) { PageRequested?.Invoke("profiles"); return true; }
        if (u.Equals("tkmm://projects", StringComparison.OrdinalIgnoreCase)) { PageRequested?.Invoke("projects"); return true; }
        if (u.Equals("tkmm://gamebanana", StringComparison.OrdinalIgnoreCase)) { PageRequested?.Invoke("gamebanana"); return true; }
        if (u.Equals("tkmm://optimizer", StringComparison.OrdinalIgnoreCase)) { PageRequested?.Invoke("optimizer"); return true; }
        if (u.Equals("tkmm://cheats", StringComparison.OrdinalIgnoreCase)) { PageRequested?.Invoke("cheats"); return true; }
        if (u.Equals("tkmm://logs", StringComparison.OrdinalIgnoreCase)) { PageRequested?.Invoke("logs"); return true; }
        if (u.Equals("tkmm://settings", StringComparison.OrdinalIgnoreCase)) { PageRequested?.Invoke("settings"); return true; }

        // Settings subsections
        const string settingsPrefix = "tkmm://settings/";
        if (u.StartsWith(settingsPrefix, StringComparison.OrdinalIgnoreCase)) {
            var section = u.Substring(settingsPrefix.Length);
            PageRequested?.Invoke("settings");
            SettingsFocusRequested?.Invoke(section);
            return true;
        }

        // Open mod in viewer, optional specific file: tkmm://mod/<modId> or tkmm://mod/<modId>/<fileId>
        if (!u.StartsWith("tkmm://mod/", StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        
        var rest = u["tkmm://mod/".Length..];
        var parts = rest.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
        if (parts.Length < 1 || !long.TryParse(parts[0], out var modId)) {
            return false;
        }
            
        long? fileId = null;
            
        if (parts.Length >= 2 && long.TryParse(parts[1], out var parsedFileId)) {
            fileId = parsedFileId;
        }
        
        if (OpenModRequested is not null) {
            _ = OpenModRequested.Invoke(modId, fileId);
        }
        
        return true;
    }

    public static void StartCli(string[] args)
    {
        var app = ConsoleApp.Create();
        ProcessBasicArgs(args);
        app.Run(args);
    }
}