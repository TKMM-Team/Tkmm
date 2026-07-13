using Avalonia.Media.Imaging;
using AvaMark;
using AvaMark.ViewModels;
using Microsoft.Extensions.Logging;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana.Helpers;

namespace Tkmm.Components;

public class TkImageResolver : IImageResolver
{
    public static readonly TkImageResolver Instance = new();

    public async ValueTask FetchImage(ImageLoadContext context, object? state, string? url)
    {
        if (url is null) {
            return;
        }
        
        try {
            if (await LoadOrDownloadAsync(url, state?.ToString() ?? "detached") is { } bitmap) {
                context.Source = bitmap;
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogWarning(ex, "Failed to resolve {ImageUrl} with {State}", url, state);
        }
    }

    public static async ValueTask<Bitmap?> LoadOrDownloadAsync(string url, string cacheTarget, CancellationToken ct = default)
    {
        if (await EnsureCachedAsync(url, cacheTarget, ct) is not { } cacheFilePath) {
            return null;
        }

        if (TryLoadBitmap(cacheFilePath) is { } bitmap) {
            return bitmap;
        }

        return await EnsureCachedAsync(url, cacheTarget, ct) is not { } retryCacheFilePath ? null : TryLoadBitmap(retryCacheFilePath);
    }

    public static async ValueTask<string?> EnsureCachedAsync(string url, string cacheTarget, CancellationToken ct = default)
    {
        var cacheFilePath = GetCacheFilePath(url, cacheTarget);

        if (File.Exists(cacheFilePath)) {
            return cacheFilePath;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(cacheFilePath)!);

        using var response = await DownloadHelper.Client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) {
            return null;
        }

        await using var src = await response.Content.ReadAsStreamAsync(ct);
        await using var ms = new MemoryStream();
        await src.CopyToAsync(ms, ct);
        await File.WriteAllBytesAsync(cacheFilePath, ms.ToArray(), ct);

        return cacheFilePath;
    }

    public static Bitmap? TryLoadBitmap(string cacheFilePath)
    {
        try {
            using var fs = File.OpenRead(cacheFilePath);
            return new Bitmap(fs);
        }
        catch (Exception ex) {
            TkLog.Instance.LogWarning(ex, "Failed to read cached image at {CacheFilePath}, re-downloading", cacheFilePath);
            try {
                File.Delete(cacheFilePath);
            }
            catch (Exception deleteEx) {
                TkLog.Instance.LogWarning(deleteEx, "Failed to delete invalid cached image at {CacheFilePath}", cacheFilePath);
            }

            return null;
        }
    }

    public static string GetCacheFilePath(string url, string cacheTarget)
        => Path.Combine(GetTempFolderPath(cacheTarget), GetFileName(url));

    private static unsafe string GetFileName(ReadOnlySpan<char> url)
    {
        Span<Range> parts = stackalloc Range[2];
        url.Split(parts, ':');
        
        var path = parts[1];

        var len = path.End.Value - path.Start.Value;
        return string.Create(len, url[path], static (result, path) => {
            for (var i = 0; i < path.Length; i++) {
                var @char = path[i];
                result[i] = @char switch {
                    '/' or '\\' => '-',
                    _ => @char
                };
            }
        });
    }

    private static string GetTempFolderPath(string target)
    {
        return Path.Combine(Path.GetTempPath(), "tkmm", "images", target);
    }

    public static void CleanTarget(string target)
    {
        var tmpFolderPath = GetTempFolderPath(target);
        
        try {
            if (!Directory.Exists(tmpFolderPath)) {
                return;
            }
            
            Directory.Delete(tmpFolderPath, recursive: true);
        }
        catch (Exception ex) {
            TkLog.Instance.LogWarning(ex,
                "Failed to cleanup {TemporaryFolderPath}", tmpFolderPath);
        }
    }
}