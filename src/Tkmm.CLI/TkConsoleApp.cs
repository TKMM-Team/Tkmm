using ConsoleAppFramework;

namespace Tkmm.CLI;

public static class TkConsoleApp
{
    public static event Func<string, Stream?, Task>? InstallRequested;

    public static event Func<long, long?, Task>? OpenModRequested;

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
        if (uri.GetComponents(UriComponents.Path, UriFormat.Unescaped) is not { } path) {
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

        if (OpenModRequested is null) {
            ShowError("Invalid State: OpenModRequest is not registered.");
            return;
        }

        if (parts is not ["mod", var modIdStr, ..]) {
            ShowError($"Invalid URI: {uri}");
            return;
        }

        if (!long.TryParse(modIdStr, out var modId)) {
            ShowError($"Invalid Mod ID: {modIdStr}");
            return;
        }

        long? fileId = 0;
        if (parts[^1] is { } fileIdStr && long.TryParse(fileIdStr, out var fileIdParsed)) {
            fileId = fileIdParsed;
        }

        _ = OpenModRequested.Invoke(modId, fileId);
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