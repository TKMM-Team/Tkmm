using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using System.Text.Json;
using Tkmm.Core;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.GameBanana;

namespace Tkmm.ViewModels.Pages;

public partial class GameBananaPageViewModel : ObservableObject
{
    private static readonly HttpClient _client = new();

    private const string GAME_ID = "7617";
    private const string FEED_ENDPOINT = $"/Game/{GAME_ID}/Subfeed?_nPage={{0}}&_sSort=new&_csvModelInclusions=Mod";

    [ObservableProperty]
    private int _page = 0;

    [ObservableProperty]
    private GameBananaFeed _feed = new();

    public GameBananaPageViewModel()
    {
        InitLoad();
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
    public static async Task InstallMod(GameBananaModInfo mod)
    {
        StackPanel panel = new() {
            Spacing = 5
        };

        panel.Children.Add(new TextBlock {
            Text = mod.Name,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        panel.Children.Add(new TextBlock {
            Text = "Choose a file to install:",
            FontSize = 11,
            Margin = new(15, 10, 0, 0),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        bool first = true;
        foreach (var file in mod.Full.Files) {
            panel.Children.Add(new RadioButton {
                GroupName = "@",
                Content = file.Name,
                IsChecked = first,
                Tag = file
            });

            first = false;
        }

        ContentDialog dialog = new() {
            Title = $"Install {mod.Name}?",
            Content = panel,
            SecondaryButtonText = "No",
            PrimaryButtonText = "Yes"
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary) {
            if (panel.Children.FirstOrDefault(x => x is RadioButton radioButton && radioButton.IsChecked == true)?.Tag is GameBananaFile file) {
                AppStatus.Set("Installing", "fa-solid fa-download", isWorkingStatus: true);
                await mod.Full.Install(file);
                AppStatus.Set("Install Complete!", "fa-regular fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
            }
        }
    }

    private async void InitLoad()
    {
        await UpdatePage();
    }

    private async Task UpdatePage()
    {
        Feed = await Fetch(Page + 1);
    }

    private static async Task<GameBananaFeed> Fetch(int page)
    {
        using Stream stream = await GameBananaHelper.Get(string.Format(FEED_ENDPOINT, page));
        GameBananaFeed feed = JsonSerializer.Deserialize<GameBananaFeed>(stream)
            ?? throw new InvalidOperationException($"Could not parse feed from '{FEED_ENDPOINT}'");

        await Task.WhenAll(feed.Records.Select(x => x.DownloadMod()));
        feed.Records = [.. feed.Records.Where(x =>
            !x.Full.IsTrashed &&
            !x.Full.IsFlagged &&
            !x.IsObsolete &&
            !x.IsContentRated &&
            !x.Full.IsPrivate
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
}
