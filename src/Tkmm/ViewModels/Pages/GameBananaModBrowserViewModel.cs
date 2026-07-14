using System.Net.NetworkInformation;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Tkmm.Components;
using Tkmm.Core;
using Tkmm.Models;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana;
using TkSharp.Extensions.GameBanana.Helpers;
using TkSharp.Extensions.GameBanana.Models;

namespace Tkmm.ViewModels.Pages;

public partial class GameBananaModBrowserViewModel : ObservableObject
{
    private const int GAME_ID = 7617;

    [ObservableProperty]
    private string _searchArgument = string.Empty;

    [ObservableProperty]
    private bool _isShowingBookmarks;

    [ObservableProperty]
    private bool _isShowingSuggested;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private double? _loadProgress;

    [ObservableProperty]
    private bool _isLoadSuccess;

    [ObservableProperty]
    private GameBananaSource _source = new(GAME_ID);

    [ObservableProperty]
    private double? _downloadSpeed;

    [ObservableProperty]
    private GameBananaFeed? _bookmarksFeed = GameBananaBookmarks.Load();

    [ObservableProperty]
    private GameBananaFeed? _suggestedModsFeed = NetworkInterface.GetIsNetworkAvailable() ? GetSuggestedFeed() : null;

    public GameBananaFeed? Feed => IsShowingBookmarks ? BookmarksFeed : IsShowingSuggested ? SuggestedModsFeed : Source.Feed;

    public bool IsFeedVisible => IsShowingBookmarks || IsShowingSuggested || IsLoadSuccess;

    public GameBananaModBrowserViewModel()
    {
        Source.PropertyChanged += (_, e) => {
            if (e.PropertyName is nameof(Source.Feed)) {
                OnPropertyChanged(nameof(Feed));
            }
        };
        
        DownloadHelper.Reporters.Push(
            new DownloadReporter {
                ProgressReporter = new Progress<double>(
                    progress => {
                        if (IsLoading) LoadProgress = progress;
                    }
                ),
                SpeedReporter = new Progress<double>(
                    speed => {
                        if (IsLoading) DownloadSpeed = speed;
                    })
            }
        );

        DownloadHelper.OnDownloadStarted += () => {
            IsLoading = true;

            return Task.CompletedTask;
        };

        DownloadHelper.OnDownloadCompleted += () => {
            IsLoading = false;
            IsLoadSuccess = true;
            LoadProgress = 0;
            DownloadSpeed = null;

            return Task.CompletedTask;
        };

        GameBananaBookmarks.Changed += ReloadBookmarksFeed;
        DownloadBookmarkThumbnails();

        _ = Refresh();

        TKMM.Config.PropertyChanged += async (_, eventArgs) => {
            if (eventArgs.PropertyName is not nameof(Config.GameBananaSortMode)) {
                return;
            }

            Source.SortMode = TKMM.Config.GameBananaSortMode;
            await Refresh();
            Config.Shared.Save();
        };
    }

    [RelayCommand]
    private async Task Refresh(ScrollViewer? modsViewer = null)
    {
        if (!NetworkInterface.GetIsNetworkAvailable()) {
            IsLoadSuccess = false;
            return;
        }
        
        await ReloadPage();
        modsViewer?.ScrollToHome();
    }

    [RelayCommand]
    public async Task Search(ScrollViewer modsViewer)
    {
        Source.CurrentPage = 0;
        await Refresh(modsViewer);
    }

    [RelayCommand]
    private async Task ResetSearch(ScrollViewer modsViewer)
    {
        Source.CurrentPage = 0;
        SearchArgument = string.Empty;
        await Refresh(modsViewer);
    }

    [RelayCommand]
    private async Task NextPage(ScrollViewer modsViewer)
    {
        Source.CurrentPage++;
        await Refresh(modsViewer);
    }

    [RelayCommand]
    private async Task PrevPage(ScrollViewer modsViewer)
    {
        Source.CurrentPage--;
        await Refresh(modsViewer);
    }

    private async Task ReloadPage()
    {
        Source.Feed?.Records.Clear();
        IsLoadSuccess = IsLoading = true;

        try {
            await Source.LoadPage(Source.CurrentPage, SearchArgument);
            await LoadFeedThumbnails(Source.Feed);
            IsLoadSuccess = true;
        }
        catch (HttpRequestException ex) {
            IsLoadSuccess = false;
            var truncatedEx = ex.ToString().Split(Environment.NewLine)[0];
            TkLog.Instance.LogWarning("An error occured when reloading page {CurrentPage} with search {SearchArgument}: {truncatedEx}",
                Source.CurrentPage, SearchArgument, truncatedEx);
            App.ToastError(ex);
        }
        catch (Exception ex) {
            IsLoadSuccess = false;
            TkLog.Instance.LogWarning("An error occured when reloading page {CurrentPage} with search {SearchArgument}: {ex}",
                Source.CurrentPage, SearchArgument, ex);
            App.ToastError(ex);
        }
        finally {
            IsLoading = false;
        }
    }

    private static GameBananaFeed? GetSuggestedFeed()
    {
        try {
            using var stream = AssetLoader.Open(new Uri("avares://Tkmm/Assets/GameBanana/Suggested.json"));
            var feed = JsonSerializer.Deserialize(stream, GameBananaFeedJsonContext.Default.GameBananaFeed)!;
            
            _ = LoadFeedThumbnails(feed);

            return feed;
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex,
                "An error occured when reading the suggested mods list. The feature will be disabled until restarting.");
            App.ToastError(ex);
        }

        return null;
    }

    partial void OnIsLoadSuccessChanged(bool value)
    {
        OnPropertyChanged(nameof(IsFeedVisible));
    }

    partial void OnIsShowingSuggestedChanged(bool value)
    {
        if (value) {
            IsShowingBookmarks = false;
            IsLoadSuccess = true;
        }

        OnPropertyChanged(nameof(Feed));
        OnPropertyChanged(nameof(IsFeedVisible));
    }

    partial void OnIsShowingBookmarksChanged(bool value)
    {
        if (value) {
            IsShowingSuggested = false;
            IsLoadSuccess = true;
        }

        OnPropertyChanged(nameof(Feed));
        OnPropertyChanged(nameof(IsFeedVisible));
    }

    private void ReloadBookmarksFeed()
    {
        BookmarksFeed = GameBananaBookmarks.Load();
        DownloadBookmarkThumbnails();

        if (IsShowingBookmarks) {
            OnPropertyChanged(nameof(Feed));
        }
    }

    private void DownloadBookmarkThumbnails()
    {
        if (BookmarksFeed is null) {
            return;
        }

        _ = LoadFeedThumbnails(BookmarksFeed);
    }

    private static Task LoadFeedThumbnails(GameBananaFeed? feed, CancellationToken ct = default)
    {
        if (feed is null || !NetworkInterface.GetIsNetworkAvailable()) {
            return Task.CompletedTask;
        }

        return Task.Run(() => Parallel.ForEachAsync(
            feed.Records, ct, static (record, ct) => new ValueTask(LoadThumbnailAsync(record, ct))
        ), ct);
    }

    private static async Task LoadThumbnailAsync(GameBananaModRecord record, CancellationToken ct = default)
    {
        if (record.ThumbnailUrl is not { } url) {
            return;
        }

        if (await TkImageResolver.LoadOrDownloadAsync(url, TkThumbnailProvider.THUMBNAIL_CACHE_TARGET, ct) is { } bitmap) {
            record.Thumbnail = bitmap;
        }
    }
}