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

        using Process process = Process.Start(info)
            ?? throw new InvalidOperationException($"Failed to start 7z from '{Config.Shared.SevenZipPath}'");
        await process.WaitForExitAsync(ct);
        
        string stdout = await process.StandardOutput.ReadToEndAsync(ct);
        TkLog.Instance.LogDebug("[External 7z Output] {StandardOutput}", stdout);
    }

    public static bool CanUseExternal()
    {
        return File.Exists(Config.Shared.SevenZipPath)
               && Process.Start(new ProcessStartInfo(Config.Shared.SevenZipPath)) is not null;
    }
}