using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TkSharp.Core;

namespace Tkmm.Core.Helpers;

// ReSharper disable once InconsistentNaming

public static class External7zHelper
{
    public static async ValueTask ExtractToFolder(string file, string output, CancellationToken ct = default)
    {
        ProcessStartInfo info = new() {
            FileName = Config.Shared.SevenZipPath,
            Arguments = $"""
                x -o"{output}" -- "{file}"
                """,
            UseShellExecute = false,
            RedirectStandardOutput = true,
        };

        using var process = Process.Start(info)
                            ?? throw new InvalidOperationException($"Failed to start 7z from '{Config.Shared.SevenZipPath}'");
        await process.WaitForExitAsync(ct);
        
        var stdout = await process.StandardOutput.ReadToEndAsync(ct);
        TkLog.Instance.LogDebug("[External 7z Output] {StandardOutput}", stdout);
    }

    public static bool CanUseExternal()
    {
#if !SWITCH
        if (string.IsNullOrWhiteSpace(Config.Shared.SevenZipPath)) {
            TryAutoSetSevenZipPath();
        }
#endif
        return File.Exists(Config.Shared.SevenZipPath)
               && Process.Start(new ProcessStartInfo(Config.Shared.SevenZipPath)) is not null;
    }
    
#if !SWITCH
    private static void TryAutoSetSevenZipPath()
    {
        if (OperatingSystem.IsWindows()) {
            foreach (var location in new[] { "ProgramFiles", "ProgramFiles(x86)" }) {
                var candidate = Path.Combine(Environment.GetEnvironmentVariable(location)!, "7-Zip", "7z.exe");
                
                if (!File.Exists(candidate)) {
                    continue;
                }
                
                Config.Shared.SevenZipPath = candidate;
                return;
            }
        }
        else {
            foreach (var binary in new[] { "7zz", "7z" }) {
                var resolved = Which(binary);
                
                if (resolved is null || !File.Exists(resolved)) {
                    continue;
                }
                
                Config.Shared.SevenZipPath = resolved;
                return;
            }
        }
    }

    private static string? Which(string name)
    {
        try {
            using var p = Process.Start(
                new ProcessStartInfo("which", name) {
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            );

            if (p is not null) {
                p.WaitForExit();
                var output = p.StandardOutput.ReadToEnd().Trim();
                return string.IsNullOrWhiteSpace(output) ? null : output;
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogWarning("Failed to run 'which' command for {Name}: {Exception}", name, ex);
        }
        return null;
    }
#endif
}