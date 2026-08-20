using ConsoleAppFramework;

namespace Tkmm.CLI;

public static class TkConsoleApp
{
    public static event Func<string, Stream?, Task>? InstallRequested;

    public static event Func<long, long?, bool, bool, Task>? OpenModRequested;

    public static event Func<int, Task>? OpenMemberRequested;

    public static event Action<string>? PageRequested;

    public static event Action<string>? SettingsFocusRequested;

    public static event Action<string>? ErrorOccurred;

    public static event Action<string, string>? PairToGameBanana;

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
    public static void ProcessArguments(string[] args)
    {
        foreach (var raw in args) {
            var arg = raw.Trim('"');

            if (arg.StartsWith("tkmm://", StringComparison.OrdinalIgnoreCase)) {
                HandleAppUri(new Uri(arg));
                continue;
            }

            if (!Path.Exists(arg) || InstallRequested is null) {
                continue;
            }

            if (File.Exists(arg)) {
                using var fs = File.OpenRead(arg);
                _ = InstallRequested.Invoke(arg, fs);
                continue;
            }

            _ = InstallRequested.Invoke(arg, null);
        }
    }

    private static void HandleAppUri(Uri uri)
    {
        if (uri.GetComponents(UriComponents.Host | UriComponents.Path, UriFormat.Unescaped) is not { } path) {
            return;
        }

        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts is [var page]) {
            PageRequested?.Invoke(page);
            return;
        }

        if (parts is ["settings", var section]) {
            PageRequested?.Invoke("settings");
            SettingsFocusRequested?.Invoke(section);
            return;
        }

        if (parts is ["pair", var key, var memberId]) {
            PairToGameBanana?.Invoke(key, memberId);
            return;
        }

        if (parts is ["member", ..]) {
            parts[0] = "members";
        }

        if (parts is ["members", var memberIdStr] && int.TryParse(memberIdStr, out var parsedMemberId) && parsedMemberId > 0) {
            if (OpenMemberRequested is null) {
                ShowError("Invalid State: OpenMemberRequested is not registered.");
                return;
            }

            _ = OpenMemberRequested.Invoke(parsedMemberId);
            return;
        }

        if (parts is ["mod", ..]) {
            parts[0] = "mods";
        }

        if (parts is ["wip", ..]) {
            parts[0] = "wips";
        }

        if (OpenModRequested is null) {
            ShowError("Invalid State: OpenModRequested is not registered.");
            return;
        }

        if (parts is not (["mods", ..] or ["wips", ..])) {
            ShowError($"Invalid URI: {uri}");
            return;
        }

        var isWip = parts[0] is "wips";

        if (parts is [_, var mode, var modIdStrA, var fileIdStrA] && long.TryParse(modIdStrA, out var modIdA) && long.TryParse(fileIdStrA, out var fileIdA)) {
            // mode can be 'install' or 'view'/'open'; 'install' sets is_silent to true
            _ = OpenModRequested.Invoke(modIdA, fileIdA, mode is "install", isWip);
            return;
        }

        if (parts.Length < 2 || !long.TryParse(parts[1], out var modId)) {
            ShowError($"Invalid Mod ID: {(parts.Length > 1 ? parts[1] : "<missing>")}");
            return;
        }

        long? fileId = 0;
        if (parts[^1] is { } fileIdStr && long.TryParse(fileIdStr, out var fileIdParsed)) {
            fileId = fileIdParsed;
        }

        _ = OpenModRequested.Invoke(modId, fileId, uri.Query.Contains("silent"), isWip);
    }

    private static void ShowError(string message)
    {
        ErrorOccurred?.Invoke(message);
    }

    public static void StartCli(string[] args)
    {
        var app = ConsoleApp.Create();
        ProcessArguments(args);
        app.Run(args);
    }
}