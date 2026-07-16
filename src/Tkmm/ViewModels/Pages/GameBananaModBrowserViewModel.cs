using System.ComponentModel;
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

    private PropertyChangedEventHandler? _sourcePropertyChangedHandler;
    private INotifyPropertyChanged? _attachedSource;

    [ObservableProperty]
    private string _searchArgument = string.Empty;

    [ObservableProperty]
    private bool _isShowingBookmarks;

    [ObservableProperty]
    private bool _isShowingSuggested;

    [ObservableProperty]
    private bool _isShowingMember;

    [ObservableProperty]
    private string? _memberUrl;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private double? _loadProgress;

    [ObservableProperty]
    private bool _isLoadSuccess;

    [ObservableProperty]
    private IGameBananaSource _source = CreateGameSource();

    [ObservableProperty]
    private double? _downloadSpeed;

    [ObservableProperty]
    private GameBananaFeed? _bookmarksFeed = GameBananaBookmarks.Load();

    [ObservableProperty]
    private GameBananaFeed? _suggestedModsFeed = NetworkInterface.GetIsNetworkAvailable() ? GetSuggestedFeed() : null;

    private CancellationTokenSource? _reloadCts;

    public GameBananaFeed? Feed => IsShowingBookmarks ? BookmarksFeed : IsShowingSuggested ? SuggestedModsFeed : Source.Feed;

    public bool IsFeedVisible => IsShowingBookmarks || IsShowingSuggested || IsLoadSuccess;

    public bool CanGoToNextPage
    {
        get
        {
            if (IsShowingBookmarks || IsShowingSuggested || Source.Feed?.Metadata is not { } metadata) {
                return false;
            }

            return Source.CurrentPage + 1 < metadata.TotalPageCount;
        }
    }

    private static bool ShowGameBananaLink =>
#if SWITCH
        false;
#else
        true;
#endif

    public bool ShowMemberLink => ShowGameBananaLink && IsShowingMember;

    public GameBananaModBrowserViewModel()
    {
        AttachSourceHandler(Source);

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
            if (eventArgs.PropertyName is not nameof(Config.GameBananaSortMode) || IsShowingMember) {
                return;
            }

            if (Source is GameBananaSource gameSource) {
                gameSource.SortMode = TKMM.Config.GameBananaSortMode;
            }

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

    public async Task OpenMemberAsync(int memberId)
    {
        SearchArgument = $"member:{memberId}";
        Source.CurrentPage = 0;
        await Refresh();
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
        IsShowingBookmarks = false;
        IsShowingSuggested = false;

        if (!TryApplySearchSource()) {
            IsLoadSuccess = false;
            IsLoading = false;
            return;
        }

        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        Interlocked.Exchange(ref _reloadCts, cts)?.Cancel();

        Source.Feed?.Records.Clear();
        IsLoadSuccess = IsLoading = true;

        try {
            var searchTerm = IsShowingMember ? null : SearchArgument;
            await Source.LoadPage(Source.CurrentPage, searchTerm, ct);
            await LoadFeedThumbnails(Source.Feed, ct);
            IsLoadSuccess = true;
            OnPropertyChanged(nameof(CanGoToNextPage));
        }
        catch (OperationCanceledException) {
            return;
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
            if (Interlocked.CompareExchange(ref _reloadCts, null, cts) == cts) {
                IsLoading = false;
            }

            cts.Dispose();
        }
    }

    private bool TryApplySearchSource()
    {
        if (TryParseMemberSearch(SearchArgument, out var memberId)) {
            IsShowingMember = true;
            MemberUrl = $"https://gamebanana.com/members/{memberId}";
            IsShowingBookmarks = false;
            IsShowingSuggested = false;

            if (Source is not GameBananaMemberSource memberSource || memberSource.MemberId != memberId) {
                Source = new GameBananaMemberSource(memberId, GAME_ID);
            }

            OnPropertyChanged(nameof(ShowMemberLink));
            return true;
        }

        if (SearchArgument.StartsWith("member:", StringComparison.OrdinalIgnoreCase)) {
            App.ToastError(new ArgumentException(Locale["GameBanana_InvalidMemberSearch"]));
            return false;
        }

        IsShowingMember = false;
        MemberUrl = null;

        if (Source is not GameBananaSource) {
            Source = CreateGameSource();
        }

        OnPropertyChanged(nameof(ShowMemberLink));
        return true;
    }

    private static bool TryParseMemberSearch(string search, out int memberId)
    {
        memberId = 0;

        if (!search.StartsWith("member:", StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        return int.TryParse(search.AsSpan(7).Trim(), out memberId) && memberId > 0;
    }

    private static GameBananaSource CreateGameSource()
        => new(GAME_ID) { SortMode = TKMM.Config.GameBananaSortMode };

    partial void OnSourceChanged(IGameBananaSource value)
        => AttachSourceHandler(value);

    partial void OnIsShowingMemberChanged(bool value)
        => OnPropertyChanged(nameof(ShowMemberLink));

    private void AttachSourceHandler(IGameBananaSource source)
    {
        if (_attachedSource is not null && _sourcePropertyChangedHandler is not null) {
            _attachedSource.PropertyChanged -= _sourcePropertyChangedHandler;
        }

        _sourcePropertyChangedHandler = (_, e) => {
            if (e.PropertyName is nameof(IGameBananaSource.Feed)) {
                OnPropertyChanged(nameof(Feed));
                OnPropertyChanged(nameof(CanGoToNextPage));
            }
            else if (e.PropertyName is nameof(IGameBananaSource.CurrentPage)) {
                OnPropertyChanged(nameof(CanGoToNextPage));
            }
        };

        if (source is INotifyPropertyChanged notifySource) {
            _attachedSource = notifySource;
            notifySource.PropertyChanged += _sourcePropertyChangedHandler;
        }
        else {
            _attachedSource = null;
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
        OnPropertyChanged(nameof(CanGoToNextPage));
    }

    partial void OnIsShowingBookmarksChanged(bool value)
    {
        if (value) {
            IsShowingSuggested = false;
            IsLoadSuccess = true;
        }

        OnPropertyChanged(nameof(Feed));
        OnPropertyChanged(nameof(IsFeedVisible));
        OnPropertyChanged(nameof(CanGoToNextPage));
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

        try {
            if (await TkImageResolver.LoadOrDownloadAsync(url, TkThumbnailProvider.THUMBNAIL_CACHE_TARGET, ct) is { } bitmap) {
                record.Thumbnail = bitmap;
            }
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception ex) {
            TkLog.Instance.LogDebug(ex, "Failed to load thumbnail for {ModName}", record.Name);
        }
    }
}
