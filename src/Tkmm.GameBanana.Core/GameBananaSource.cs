using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.GameBanana.Core;

public sealed partial class GameBananaSource(int gameId) : ObservableObject, IGameBananaSource
{
    private readonly int _gameId = gameId;

    [ObservableProperty]
    private int _currentPage;

    [ObservableProperty]
    private GameBananaSortMode _sortMode;

    [ObservableProperty]
    private GameBananaFeed? _feed;
    
    public async ValueTask LoadPage(int page, string? searchTerm = null, GameBananaFeed? customFeed = null, CancellationToken ct = default)
    {
        string sort = SortMode.ToString().ToLower();

        if (customFeed is null) {
            Feed = new GameBananaFeed();
            await GameBanana.FillFeed(Feed, _gameId, page, sort, searchTerm, ct);
        }
        else {
            Feed = customFeed;
        }

        await FilterRecords(Feed, ct);
        _ = DownloadThumbnails(Feed, ct);
    }

    private static async ValueTask FilterRecords(GameBananaFeed feed, CancellationToken ct)
    {
        for (int i = 0; i < feed.Records.Count; i++) {
            GameBananaModRecord record = feed.Records[i];
            await record.DownloadFullMod(ct);
            
            if (ct.IsCancellationRequested) {
                break;
            }

            bool isRecordClean = record is {
                Full: {
                    IsTrashed: false, IsFlagged: false, IsPrivate: false
                },
                IsObsolete: false, IsContentRated: false
            };

            if (isRecordClean) {
                continue;
            }
            
            feed.Records.RemoveAt(i);
            i--;
        }
    }

    private static Task DownloadThumbnails(GameBananaFeed feed, CancellationToken ct)
    {
        return Task.Run(() => Parallel.ForEachAsync(
            feed.Records, ct, static (record, ct) => record.DownloadThumbnail(ct)
        ), ct);
    }
}