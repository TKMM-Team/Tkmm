using Avalonia.Media.Imaging;
using AvaMark;
using AvaMark.ViewModels;
using Microsoft.Extensions.Logging;
using TkSharp.Core;

namespace Tkmm.Components;

public class TkImageResolver : IImageResolver
{
    private static readonly HttpClient _httpClient = new(new SocketsHttpHandler {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
    });

    public static readonly TkImageResolver Instance = new();

    public async ValueTask FetchImage(ImageLoadContext context, object? state, string? url)
    {
        if (url is null) {
            return;
        }
        
        try {
            string tmpFolderPath = GetTempFolderPath(state?.ToString() ?? "detached");
            string tmpFileName = Path.Combine(tmpFolderPath, GetFileName(url));

            if (File.Exists(tmpFileName)) {
                context.Source = new Bitmap(tmpFileName);
                return;
            }

            Directory.CreateDirectory(tmpFolderPath);

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) {
                return;
            }

            await using FileStream fs = File.Create(tmpFileName);
            await using Stream src = await response.Content.ReadAsStreamAsync();
            await src.CopyToAsync(fs);

            fs.Seek(0, SeekOrigin.Begin);
            context.Source = new Bitmap(fs);
        }
        catch (Exception ex) {
            TkLog.Instance.LogWarning(ex, "Failed to resolve {ImageUrl} with {State}", url, state);
        }
    }

    private static unsafe string GetFileName(ReadOnlySpan<char> url)
    {
        Span<Range> parts = stackalloc Range[2];
        url.Split(parts, ':');
        
        Range path = parts[1];

        int len = path.End.Value - path.Start.Value;
        return string.Create(len, url[path], static (result, path) => {
            for (int i = 0; i < path.Length; i++) {
                char @char = path[i];
                result[i] = @char switch {
                    '/' or '\\' => '-',
                    _ => @char
                };
            }
        });
    }

    public static string GetTempFolderPath(string target)
    {
        return Path.Combine(Path.GetTempPath(), "tkmm", "images", target);
    }

    public static void CleanTarget(string target)
    {
        string tmpFolderPath = GetTempFolderPath(target);
        
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