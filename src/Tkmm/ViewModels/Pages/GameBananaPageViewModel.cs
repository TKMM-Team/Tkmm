using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using Tkmm.Core;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.GameBanana;
using Tkmm.Helpers;
using Tkmm.Views.Common;

namespace Tkmm.ViewModels.Pages;

public partial class GameBananaPageViewModel : ObservableObject
{
    private static readonly HttpClient _client = new();

    private const string GAME_ID = "7617";
    private const string FEED_ENDPOINT = $"/Game/{GAME_ID}/Subfeed?_nPage={{0}}&_sSort={{1}}&_csvModelInclusions=Mod";
    private const string FEED_ENDPOINT_SEARCH = $"/Game/{GAME_ID}/Subfeed?_nPage={{0}}&_sSort={{1}}&_sName={{2}}&_csvModelInclusions=Mod";

    private static GameBananaFeed? _sugesstedModsFeed = GetSuggestedFeed();

    [ObservableProperty]
    private string _searchArgument = string.Empty;

    [ObservableProperty]
    private int _page = 0;

    [ObservableProperty]
    private bool _isShowingSuggested = false;

    [ObservableProperty]
    private GameBananaFeed _feed = new();

    public GameBananaPageViewModel()
    {
        InitLoad();

        Config.Shared.PropertyChanged += async (s, e) => {
            if (e.PropertyName == nameof(Config.GameBananaSortMode)) {
                await UpdatePage();
                Config.Shared.Save();
            }
        };
    }

    [RelayCommand]
    public async Task Search(ScrollViewer modsViewer)
    {
        Page = 0;
        await UpdatePage();
        modsViewer.ScrollToHome();
    }

    [RelayCommand]
    public async Task ResetSearch(ScrollViewer modsViewer)
    {
        Page = 0;
        SearchArgument = string.Empty;
        await UpdatePage();
        modsViewer.ScrollToHome();
    }

    [RelayCommand]
    public async Task NextPage(ScrollViewer modsViewer)
    {
        Page++;
        await UpdatePage();
        modsViewer.ScrollToHome();
    }

    [RelayCommand]
    public async Task PrevPage(ScrollViewer modsViewer)
    {
        Page--;
        await UpdatePage();
        modsViewer.ScrollToHome();
    }

    [RelayCommand]
    public async Task ShowSuggested(ScrollViewer modsViewer)
    {
        _sugesstedModsFeed ??= GetSuggestedFeed();

        if (IsShowingSuggested == false) {
            await UpdatePage();
            modsViewer.ScrollToHome();
            return;
        }

        await UpdatePage(_sugesstedModsFeed);
    }

    [RelayCommand]
    public static async Task InstallMod(GameBananaModInfo mod)
    {
        ArgumentNullException.ThrowIfNull(mod.Full, nameof(mod.Full));

        GameBananaInstallPreview preview = new() {
            DataContext = mod
        };

        GameBananaFile? target = mod.Full.Files.FirstOrDefault(x => x.IsSelected);

        ContentDialog dialog = new() {
            Title = $"Install {mod.Name}",
            Content = preview,
            SecondaryButtonText = "Cancel",
            PrimaryButtonText = "Install",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = target is not null,
        };

        foreach (GameBananaFile file in mod.Full.Files) {
            file.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(file.IsSelected)) {
                    target = file;
                    dialog.IsPrimaryButtonEnabled = true;
                }
            };
        }

        if (await dialog.ShowAsync() == ContentDialogResult.Primary && target is not null) {
            AppStatus.Set($"Downloading '{target.Name}'", "fa-solid fa-download", isWorkingStatus: true);
            await ModHelper.Import(target, mod.Full.FromFile);
        }
    }

    private async void InitLoad()
    {
        await UpdatePage();
    }

    private async Task UpdatePage(GameBananaFeed? customFeed = null)
    {
        if (customFeed is null) {
            IsShowingSuggested = false;
        }

        try {
            Feed = await Fetch(Page + 1, SearchArgument, customFeed);
        }
        catch (Exception ex) {
            AppLog.Log(ex);
            App.ToastError(ex);
            Feed = await Fetch((Page = 0) + 1, string.Empty);
        }
    }

    private static async Task<GameBananaFeed> Fetch(int page, string search, GameBananaFeed? customFeed = null)
    {
        string sort = Config.Shared.GameBananaSortMode.ToString().ToLower();
        search = search.Trim();
        string endpoint = !string.IsNullOrEmpty(search) && search.Length > 2
            ? string.Format(FEED_ENDPOINT_SEARCH, page, sort, search)
            : string.Format(FEED_ENDPOINT, page, sort);

        using Stream stream = await GameBananaHelper.Get(endpoint);
        GameBananaFeed feed = customFeed ?? JsonSerializer.Deserialize<GameBananaFeed>(stream)
            ?? throw new InvalidOperationException($"Could not parse feed from '{FEED_ENDPOINT}'");

        await Task.WhenAll(feed.Records.Select(x => x.FetchMetadata()));
        feed.Records = [.. feed.Records.Where(x =>
            x.Full?.IsTrashed == false &&
            x.Full?.IsFlagged == false &&
            x.IsObsolete == false &&
            x.IsContentRated == false &&
            x.Full?.IsPrivate == false
        )];

        _ = Task.Run(() => DownloadThumbnails(feed));
        return feed;
    }

    private static async Task DownloadThumbnails(GameBananaFeed feed)
    {
        foreach (var mod in feed.Records) {
            if (mod.Media.Images.FirstOrDefault() is GameBananaImage img) {
                byte[] image = await _client
                    .GetByteArrayAsync($"{img.BaseUrl}/{img.SmallFile}");
                using MemoryStream ms = new(image);
                mod.Thumbnail = new Bitmap(ms);
            }
        }
    }

    private static GameBananaFeed? GetSuggestedFeed()
    {
        string path = Path.Combine(Config.Shared.StaticStorageFolder, "suggested.json");
        if (File.Exists(path)) {
            using FileStream fs = File.OpenRead(path);
            return JsonSerializer.Deserialize<GameBananaFeed>(fs);
        }

        return null;
    }
}
