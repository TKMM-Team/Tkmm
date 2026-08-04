#if !SWITCH
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Tkmm.Components;

namespace Tkmm.Helpers;

public static class TempFolderGuard
{
    public static bool IsRunningFromTemporaryFolder()
    {
        if (AppUpdater.IsAppImage) {
            return false;
        }

        foreach (var candidate in GetPathsToCheck()) {
            if (IsUnderTemporaryFolder(candidate)) {
                return true;
            }
        }

        return false;
    }

    public static void Apply(Window shellView)
    {
        if (shellView.Content is Control content) {
            content.IsEnabled = false;
        }

        shellView.Opened += OnShellOpened;
    }

    private static async void OnShellOpened(object? sender, EventArgs e)
    {
        if (sender is Window window) {
            window.Opened -= OnShellOpened;
        }

        await Dispatcher.UIThread.InvokeAsync(ShowBlockingErrorAndExit);
    }

    private static async Task ShowBlockingErrorAndExit()
    {
        ContentDialog dialog = new() {
            Title = Locale["TempFolderGuard_Title"],
            Content = Locale["TempFolderGuard_Message"],
            PrimaryButtonText = Locale["TempFolderGuard_Close"],
            DefaultButton = ContentDialogButton.Primary
        };

        try {
            await dialog.ShowAsync();
        }
        finally {
            Exit();
        }
    }

    private static void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.Shutdown(1);
            return;
        }

        Environment.Exit(1);
    }

    private static IEnumerable<string> GetPathsToCheck()
    {
        var processPath = Environment.ProcessPath;
        if (processPath is not null) {
            var processName = Path.GetFileNameWithoutExtension(processPath);
            if (!processName.Equals("dotnet", StringComparison.OrdinalIgnoreCase)
                && Path.GetDirectoryName(processPath) is { } processDir) {
                yield return processDir;
                yield break;
            }
        }

        yield return AppContext.BaseDirectory;
    }

    private static bool IsUnderTemporaryFolder(string path)
    {
        string fullPath;
        try {
            fullPath = Path.GetFullPath(path);
        }
        catch {
            return false;
        }

        foreach (var tempRoot in GetTemporaryRoots()) {
            if (IsSubPathOf(fullPath, tempRoot)) {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> GetTemporaryRoots()
    {
        yield return Path.GetTempPath();

        if (OperatingSystem.IsWindows()) {
            foreach (var name in (string[]) ["TMP", "TEMP"]) {
                if (Environment.GetEnvironmentVariable(name) is { Length: > 0 } value) {
                    yield return value;
                }
            }

            yield break;
        }

        if (Environment.GetEnvironmentVariable("TMPDIR") is { Length: > 0 } tmpDir) {
            yield return tmpDir;
        }

        yield return "/tmp";
        yield return "/var/tmp";
    }

    private static bool IsSubPathOf(string path, string potentialParent)
    {
        string fullParent;
        try {
            fullParent = Path.GetFullPath(potentialParent)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch {
            return false;
        }

        if (fullParent.Length == 0) {
            return false;
        }

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (path.Equals(fullParent, comparison)) {
            return true;
        }

        var prefix = fullParent + Path.DirectorySeparatorChar;
        return path.StartsWith(prefix, comparison);
    }
}
#endif