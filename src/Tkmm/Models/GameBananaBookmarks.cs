using System.Text.Json;
using Tkmm.Core;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.Models;

public static class GameBananaBookmarks
{
    private static readonly string FilePath = Path.Combine(TKMM.BaseDirectory, ".data2", "bookmarks.json");

    private static GameBananaFeed? _cache;

    public static event Action? Changed;

    public static GameBananaFeed Load()
    {
        if (_cache is not null) {
            return _cache;
        }

        try {
            if (!File.Exists(FilePath)) {
                return _cache = CreateEmptyFeed();
            }

            using var stream = File.OpenRead(FilePath);
            return _cache = JsonSerializer.Deserialize(stream, GameBananaFeedJsonContext.Default.GameBananaFeed)
                ?? CreateEmptyFeed();
        }
        catch {
            return _cache = CreateEmptyFeed();
        }
    }

    public static bool IsBookmarked(int modId)
        => Load().Records.Any(record => record.Id == modId);

    public static void Toggle(GameBananaMod mod)
    {
        _cache = null;
        var feed = Load();
        var modId = (int)mod.Id;
        var existing = feed.Records.FirstOrDefault(record => record.Id == modId);

        if (existing is not null) {
            feed.Records.Remove(existing);
        }
        else {
            feed.Records.Insert(0, FromMod(mod));
        }

        Save(feed);
    }

    private static GameBananaModRecord FromMod(GameBananaMod mod)
        => new() {
            Id = (int)mod.Id,
            Name = mod.Name,
            Url = $"https://gamebanana.com/mods/{mod.Id}",
            Media = mod.Media,
            Submitter = mod.Submitter,
            Version = mod.Version,
        };

    private static void Save(GameBananaFeed feed)
    {
        feed.Metadata.IsCompleted = true;

        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        using var stream = File.Create(FilePath);
        JsonSerializer.Serialize(stream, feed, GameBananaFeedJsonContext.Default.GameBananaFeed);
        _cache = feed;
        Changed?.Invoke();
    }

    private static GameBananaFeed CreateEmptyFeed()
        => new() {
            Metadata = new GameBananaMetadata {
                IsCompleted = true
            }
        };
}
