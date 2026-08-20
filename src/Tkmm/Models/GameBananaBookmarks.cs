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

    public static bool IsBookmarked(int submissionId)
        => Load().Records.Any(record => record.Id == submissionId);

    public static void Toggle(GameBananaSubmission submission)
    {
        _cache = null;
        var feed = Load();
        var submissionId = (int)submission.Id;
        var existing = feed.Records.FirstOrDefault(record => record.Id == submissionId);

        if (existing is not null) {
            feed.Records.Remove(existing);
        }
        else {
            feed.Records.Insert(0, FromSubmission(submission));
        }

        Save(feed);
    }

    private static GameBananaSubmissionRecord FromSubmission(GameBananaSubmission submission)
        => new() {
            Id = (int)submission.Id,
            Name = submission.Name,
            Url = submission.ProfileUrl,
            Media = submission.Media,
            Submitter = submission.Submitter,
            Version = submission.Version,
            Type = submission.Type,
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
