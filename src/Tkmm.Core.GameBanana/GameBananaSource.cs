using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.GameBanana;

public sealed partial class GameBananaSource(int gameId) : ObservableObject, IGameBananaSource
{
    private readonly int _gameId = gameId;

    [ObservableProperty]
    private int _currentPage;

    [ObservableProperty]
    private GameBananaSortMode _sortMode;

    [ObservableProperty]
    private GameBananaFeed? _feed;

    public ValueTask Search(string searchTerm, CancellationToken ct)
    {
        return LoadPage(CurrentPage = 0, searchTerm, customFeed: null, ct);
    }
    
    public ValueTask LoadPage(int page, GameBananaFeed? customFeed, CancellationToken ct)
    {
        return LoadPage(page, null, customFeed, ct);
    }
    
    private async ValueTask LoadPage(int page, string? searchTerm, GameBananaFeed? customFeed, CancellationToken ct)
    {
        string sort = SortMode.ToString().ToLower();
        GameBananaFeed? feed = customFeed ?? await GameBanana.GetFeed(_gameId, page + 1, sort, searchTerm, ct);

        if (feed is null) {
            goto UpdateFeed; 
        }

        await FilterRecords(feed, ct);
        _ = DownloadThumbnails(feed, ct);

    UpdateFeed:
        Feed = feed;
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