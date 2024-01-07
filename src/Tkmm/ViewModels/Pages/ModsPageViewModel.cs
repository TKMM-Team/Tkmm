using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.GameBanana;

namespace Tkmm.ViewModels.Pages;

public partial class ModsPageViewModel : ObservableObject
{
    private static readonly HttpClient _client = new();

    private const string GAME_ID = "7617";
    private const string ENDPOINT = $"/Game/{GAME_ID}/Subfeed?_nPage={{0}}&_sSort=new&_csvModelInclusions=Mod";

    private GameBananaFeed? _nextFeed;

    [ObservableProperty]
    private int _page = 0;

    [ObservableProperty]
    private GameBananaFeed _feed = new();

    public ModsPageViewModel()
    {
        InitLoad();
    }

    [RelayCommand]
    public async Task NextPage()
    {
        Page++;
        await UpdatePage();
    }

    [RelayCommand]
    public async Task PrevPage()
    {
        Page--;
        await UpdatePage();
    }

    private async void InitLoad()
    {
        await UpdatePage();
    }

    private async Task UpdatePage()
    {
        Feed = _nextFeed ?? await Fetch(Page + 1);
    }

    private static async Task<GameBananaFeed> Fetch(int page)
    {
        using Stream stream = await GameBanana.Get(string.Format(ENDPOINT, page));
        GameBananaFeed feed = JsonSerializer.Deserialize<GameBananaFeed>(stream)
            ?? throw new InvalidOperationException($"Could not parse feed from '{ENDPOINT}'");
        _ = Task.Run(() => DownloadThumbnails(feed));
        return feed;
    }

    private static async Task DownloadThumbnails(GameBananaFeed feed)
    {
        foreach (var mod in feed.Records) {
            if (mod.Media.Images.FirstOrDefault() is GameBananaImage img) {
                byte[] image = await _client
                    .GetByteArrayAsync($"{img.BaseUrl}/{img.File}");
                using MemoryStream ms = new(image);
                mod.Thumbnail = new Bitmap(ms);
            }
        }
    }
}
