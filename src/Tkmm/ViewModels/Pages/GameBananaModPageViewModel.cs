using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Actions;
using Tkmm.Components;
using Tkmm.Models;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.ViewModels.Pages;

public partial class GameBananaModPageViewModel : ObservableObject
{
    private const string GAME_BANANA_STATIC_CACHE_TARGET = "gamebanana";

    private CancellationTokenSource? _loadingCts;

    [ObservableProperty]
    public partial GameBananaMod? SelectedMod { get; set; }

    [ObservableProperty]
    public partial GameBananaMod? MarkdownMod { get; set; }

    [ObservableProperty]
    private int _selectedImageIndex;

    [ObservableProperty]
    private ObservableCollection<object> _images = [];

    [ObservableProperty]
    public partial object? BananaIcon { get; set; }

    [ObservableProperty]
    private bool _isLoading;

    public static bool ShowGameBananaLink => 
#if SWITCH
        false;
#else
        true;
#endif

    partial void OnSelectedImageIndexChanged(int value)
    {
        NotifyImagePropertiesChanged();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowLoadingImagesMessage));
        OnPropertyChanged(nameof(ShowNoImagesMessage));
    }

    private void NotifyImagePropertiesChanged()
    {
        OnPropertyChanged(nameof(SelectedImage));
        OnPropertyChanged(nameof(CanGoToPreviousImage));
        OnPropertyChanged(nameof(CanGoToNextImage));
        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(ShowLoadingImagesMessage));
        OnPropertyChanged(nameof(ShowNoImagesMessage));
        OnPropertyChanged(nameof(ShowNoThumbnailMessage));
    }

    public string FormattedDateAdded => SelectedMod?.DateAdded > 0
        ? DateTimeOffset.FromUnixTimeSeconds(SelectedMod.DateAdded).ToString("MMM d, yyyy")
        : string.Empty;

    public string FormattedDateUpdated => SelectedMod?.DateUpdated > 0
        ? DateTimeOffset.FromUnixTimeSeconds(SelectedMod.DateUpdated).ToString("MMM d, yyyy")
        : string.Empty;

    public bool HasImages => Images.Count > 0;

    public bool ShowLoadingImagesMessage => IsLoading && Images.Count == 0;

    public bool ShowNoImagesMessage => !IsLoading && Images.Count == 0;

    public bool ShowNoThumbnailMessage => Images.Count == 0;

    public bool CanGoToPreviousImage => Images.Count > 1 && SelectedImageIndex > 0;

    public bool CanGoToNextImage => Images.Count > 1 && SelectedImageIndex < Images.Count - 1;

    public object? SelectedImage => Images.Count > 0 && SelectedImageIndex >= 0 && SelectedImageIndex < Images.Count
        ? Images[SelectedImageIndex]
        : null;

    public string ModUrl => SelectedMod != null ? $"https://gamebanana.com/mods/{SelectedMod.Id}" : string.Empty;

    public bool IsBookmarked => SelectedMod is { } mod && GameBananaBookmarks.IsBookmarked((int)mod.Id);

    public string BookmarkIcon => IsBookmarked ? "fa-solid fa-star" : "fa-regular fa-star";

    public string BookmarkLabel => Locale[IsBookmarked ? "GameBanana_RemoveBookmark" : "GameBanana_BookmarkMod"];

    public static GameBananaModPageViewModel CreateForMod(GameBananaMod mod, GameBananaModBrowserViewModel? browser = null)
    {
        var viewer = new GameBananaModPageViewModel();
        viewer.PrepareMod(mod);
        return viewer;
    }

    private void PrepareMod(GameBananaMod mod)
    {
        CancelLoading();

        SelectedMod = mod;
        MarkdownMod = null;
        IsLoading = true;
        Images.Clear();
        SelectedImageIndex = 0;
        BananaIcon = GameBananaPageViewModel.Icon;

        OnPropertyChanged(nameof(SelectedMod));
        OnPropertyChanged(nameof(ModUrl));
        OnPropertyChanged(nameof(IsBookmarked));
        OnPropertyChanged(nameof(BookmarkIcon));
        OnPropertyChanged(nameof(BookmarkLabel));
        OnPropertyChanged(nameof(FormattedDateAdded));
        OnPropertyChanged(nameof(FormattedDateUpdated));
        NotifyImagePropertiesChanged();
    }

    public void CancelLoading()
    {
        if (_loadingCts is not { } cts) {
            return;
        }

        cts.Cancel();
        cts.Dispose();
        _loadingCts = null;
    }

    public async Task StartLoadingAsync()
    {
        if (SelectedMod is null) {
            return;
        }

        CancelLoading();
        _loadingCts = new CancellationTokenSource();
        var ct = _loadingCts.Token;

        try {
            _ = LoadBananaIconAsync(ct);

            await Dispatcher.UIThread.InvokeAsync(() => MarkdownMod = SelectedMod, DispatcherPriority.Background);

            await LoadImagesAsync(ct);

            await Dispatcher.UIThread.InvokeAsync(() => {
                IsLoading = false;
                NotifyImagePropertiesChanged();
            }, DispatcherPriority.Background);

            if (ct.IsCancellationRequested) {
                return;
            }

            _ = LoadAvatarsAsync(ct);
        }
        catch (OperationCanceledException) {
            // Viewer was closed or replaced before loading finished.
        }
    }

    public void Reset()
    {
        CancelLoading();

        if (SelectedMod?.Credits is { } credits) {
            foreach (var author in credits.SelectMany(creditGroup => creditGroup.Authors)) {
                author.LoadedAvatar = null;
            }
        }

        SelectedMod = null;
        MarkdownMod = null;
        Images.Clear();
        SelectedImageIndex = 0;
        IsLoading = false;

        OnPropertyChanged(nameof(ModUrl));
        OnPropertyChanged(nameof(FormattedDateAdded));
        OnPropertyChanged(nameof(FormattedDateUpdated));
        NotifyImagePropertiesChanged();
    }

    [RelayCommand]
    private void NextImage()
    {
        if (Images.Count > 1) {
            SelectedImageIndex = (SelectedImageIndex + 1) % Images.Count;
        }
    }

    [RelayCommand]
    private void PreviousImage()
    {
        if (Images.Count > 1) {
            SelectedImageIndex = SelectedImageIndex == 0 ? Images.Count - 1 : SelectedImageIndex - 1;
        }
    }

    [RelayCommand]
    private void SelectImage(object? image)
    {
        if (image == null || !Images.Contains(image)) {
            return;
        }

        SelectedImageIndex = Images.IndexOf(image);
    }

    [RelayCommand]
    private async Task InstallMod(GameBananaFile file)
    {
        if (SelectedMod is null) {
            return;
        }

        TkStatus.Set($"Downloading '{file.Name}'", "fa-solid fa-download", StatusType.Working);
        await ModActions.Instance.Install((SelectedMod, file));
    }

    [RelayCommand]
    private void ToggleBookmark()
    {
        if (SelectedMod is null) {
            return;
        }

        GameBananaBookmarks.Toggle(SelectedMod);
        OnPropertyChanged(nameof(IsBookmarked));
        OnPropertyChanged(nameof(BookmarkIcon));
        OnPropertyChanged(nameof(BookmarkLabel));
    }

    private Task LoadBananaIconAsync(CancellationToken ct)
    {
        return BananaIcon is not null ? Task.CompletedTask : AssignBananaIconAsync(ct);
    }

    private async Task AssignBananaIconAsync(CancellationToken ct)
    {
        if (await GameBananaPageViewModel.LoadBananaIconAsync(ct) is { } icon) {
            await Dispatcher.UIThread.InvokeAsync(() => BananaIcon = icon);
        }
    }

    private async Task LoadAvatarsAsync(CancellationToken ct)
    {
        if (SelectedMod?.Credits is not null) {
            foreach (var author in SelectedMod.Credits.SelectMany(creditGroup => creditGroup.Authors)) {
                ct.ThrowIfCancellationRequested();
                await LoadSingleAvatarAsync(author, ct);
            }
        }
    }

    private static async Task LoadSingleAvatarAsync(GameBananaAuthor author, CancellationToken ct)
    {
        var avatarUrl = !string.IsNullOrEmpty(author.AvatarUrl)
            ? author.AvatarUrl
            : "https://images.gamebanana.com/static/img/defaults/avatar.gif";

        if (await TkImageResolver.EnsureCachedAsync(avatarUrl, GAME_BANANA_STATIC_CACHE_TARGET, ct) is not { } cacheFilePath) {
            return;
        }

        if (TkImageResolver.TryLoadBitmap(cacheFilePath) is not { } bitmap) {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() => author.LoadedAvatar = bitmap, DispatcherPriority.Background);
    }

    private async Task LoadImagesAsync(CancellationToken ct)
    {
        await Dispatcher.UIThread.InvokeAsync(() => {
            Images.Clear();
            SelectedImageIndex = 0;
            NotifyImagePropertiesChanged();
        }, DispatcherPriority.Background);

        if (SelectedMod?.Media.Images is not { } images) {
            return;
        }

        var cacheTarget = SelectedMod.Id.ToString();
        foreach (var image in images) {
            ct.ThrowIfCancellationRequested();

            if (await TkImageResolver.EnsureCachedAsync($"{image.BaseUrl}/{image.File}", cacheTarget, ct) is not { } cacheFilePath) {
                continue;
            }

            if (TkImageResolver.TryLoadBitmap(cacheFilePath) is not { } bitmap) {
                continue;
            }

            await Dispatcher.UIThread.InvokeAsync(() => {
                Images.Add(bitmap);
                NotifyImagePropertiesChanged();
            }, DispatcherPriority.Background);

            await Task.Yield();
        }
    }
}
