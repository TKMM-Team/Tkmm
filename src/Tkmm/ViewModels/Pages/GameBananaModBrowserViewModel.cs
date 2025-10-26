using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Tkmm.Actions;
using Tkmm.Core;
using Tkmm.Views.Common;
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
    private GameBananaFeed? _suggestedModsFeed = InternetHelper.HasInternet ? GetSuggestedFeed() : null;

    public GameBananaFeed? Feed => IsShowingSuggested ? SuggestedModsFeed : Source.Feed;

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
        if (!InternetHelper.HasInternet) {
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

    [RelayCommand]
    public static async Task InstallMod(GameBananaModRecord mod)
    {
        if (mod.Full is null) {
            await mod.DownloadFullMod();
        }

        GameBananaInstallPreview preview = new() {
            DataContext = mod
        };

        var target = mod.Full?.Files
            .FirstOrDefault(file => file.IsSelected);

        ContentDialog dialog = new() {
            Title = $"Install {mod.Name}",
            Content = preview,
            SecondaryButtonText = "Cancel",
            PrimaryButtonText = "Install",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = target is not null,
        };

        foreach (var file in mod.Full!.Files) {
            file.PropertyChanged += (_, eventArgs) => {
                if (eventArgs.PropertyName == nameof(file.IsSelected)) {
                    target = file;
                    dialog.IsPrimaryButtonEnabled = true;
                }
            };
        }

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary || target is null) {
            return;
        }

        TkStatus.Set($"Downloading '{target.Name}'", "fa-solid fa-download", StatusType.Working);
        await ModActions.Instance.Install((mod.Full, target));
    }

    private async Task ReloadPage()
    {
        Source.Feed?.Records.Clear();
        IsLoadSuccess = IsLoading = true;

        try {
            await Source.LoadPage(Source.CurrentPage, SearchArgument);
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
            
            _ = Task.Run(() => Parallel.ForEachAsync(
                feed.Records, static (record, ct) => record.DownloadThumbnail(ct)
            ));

            return feed;
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex,
                "An error occured when reading the suggested mods list. The feature will be disabled until restarting.");
            App.ToastError(ex);
        }

        return null;
    }

    partial void OnIsShowingSuggestedChanged(bool value)
    {
        OnPropertyChanged(nameof(Feed));
    }
}