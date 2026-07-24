using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using Tkmm.Core.Providers;
using TkSharp;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Components;

public sealed class TkThumbnailProvider(Bitmap defaultThumbnail) : ITkThumbnailProvider
{
    public const string THUMBNAIL_CACHE_TARGET = "thumbnails";
    
    public static readonly TkThumbnailProvider Instance;

    static TkThumbnailProvider()
    {
        using var defaultThumbnailBitmapStream = AssetLoader.Open(new Uri("avares://Tkmm/Assets/DefaultThumbnail.jpg"));
        Bitmap defaultThumbnailBitmap = new(defaultThumbnailBitmapStream);
        Instance = new TkThumbnailProvider(defaultThumbnailBitmap);
    }
    
    private readonly Bitmap _defaultThumbnail = defaultThumbnail;

    public async Task ResolveThumbnail(TkMod mod, CancellationToken ct = default)
    {
        var src = mod.Changelog.Source;
        await ResolveThumbnail(mod, src, useDefault: true, ct);

        if (src is null) {
            return;
        }

        foreach (var group in mod.OptionGroups) {
            await ResolveThumbnail(group, src, useDefault: false, ct);

            foreach (var option in group.Options) {
                await ResolveThumbnail(option, src, useDefault: false, ct);
            }
        }
    }

    private async Task ResolveThumbnail(TkItem item, ITkSystemSource? src, bool useDefault = false, CancellationToken ct = default)
    {
        var thumbnail = item.Thumbnail;
        
        if (thumbnail is null || src is null) {
            goto UseDefault;
        }

        if (thumbnail.RelativeThumbnailPath is { } relativePath && src.Exists(relativePath)) {
            if (await TryLoadFromSource(thumbnail, src, relativePath)) {
                return;
            }
        }

        if (!IsRemoteUrl(thumbnail.ThumbnailPath) && src.Exists(thumbnail.ThumbnailPath)) {
            if (await TryLoadFromSource(thumbnail, src, thumbnail.ThumbnailPath)) {
                return;
            }
        }
        
        if (!Uri.TryCreate(thumbnail.ThumbnailPath, UriKind.Absolute, out var uri)) {
            goto UseDefault;
        }

        var url = uri.ToString();
        var cacheFilePath = TkImageResolver.GetCacheFilePath(url, THUMBNAIL_CACHE_TARGET);

        if (!File.Exists(cacheFilePath) && !NetworkInterface.GetIsNetworkAvailable()) {
            goto UseDefault;
        }

        try {
            if (await TkImageResolver.LoadOrDownloadAsync(url, THUMBNAIL_CACHE_TARGET, ct) is { } bitmap) {
                thumbnail.Bitmap = bitmap;
                return;
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Failed to resolve thumbnail URI '{Uri}'", uri);
        }
        
    UseDefault:
        if (!useDefault) {
            return;
        }
        
        if (item.Thumbnail is { ThumbnailPath.Length: > 0 } existingThumbnail) {
            existingThumbnail.Bitmap = _defaultThumbnail;
            existingThumbnail.IsDefault = false;
            return;
        }
        
        item.Thumbnail = new TkThumbnail {
            Bitmap = _defaultThumbnail,
            IsDefault = true
        };
    }

    private static async Task<bool> TryLoadFromSource(TkThumbnail thumbnail, ITkSystemSource src, string path)
    {
        try {
            await using var imageStream = src.OpenRead(path);
            thumbnail.Bitmap = new Bitmap(imageStream);
            return true;
        }
        catch {
            return false;
        }
    }

    private static bool IsRemoteUrl(string path)
        => Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
}