using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using System.Text.Json;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.GameBanana;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Tkmm.ViewModels.Pages;

public partial class GameBananaPageViewModel : ObservableObject
{
    private static readonly HttpClient _client = new();

    private const string GAME_ID = "7617";
    private const string FEED_ENDPOINT = $"/Game/{GAME_ID}/Subfeed?_nPage={{0}}&_csvModelInclusions=Mod";
    private const string FEED_ENDPOINT_SEARCH = $"/Game/{GAME_ID}/Subfeed?_nPage={{0}}&_sName={{1}}&_csvModelInclusions=Mod";

    [ObservableProperty]
    private string _searchArgument = string.Empty;

    [ObservableProperty]
    private int _page = 0;

    [ObservableProperty]
    private GameBananaFeed _feed = new();

    [ObservableProperty]
    private string _userInput = string.Empty;

    private bool _includeContentRated = false;

    public GameBananaPageViewModel()
    {
        InitLoad();
    }

    [RelayCommand]
    public async Task Search(ScrollViewer modsViewer)
    {
        if (SearchArgument.ToUpper() == "SECRETNSFW")
        {
            _includeContentRated = true;
            SearchArgument = string.Empty;
        }

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
    public static async Task InstallMod(GameBananaModInfo mod)
    {
        StackPanel panel = new()
        {
            Spacing = 5
        };

        panel.Children.Add(new TextBlock
        {
            Text = mod.Name,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        panel.Children.Add(new TextBlock
        {
            Text = "Choose a file to install:",
            FontSize = 11,
            Margin = new(15, 10, 0, 0),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        bool first = true;
        foreach (var file in mod.Full.Files)
        {
            panel.Children.Add(new RadioButton
            {
                GroupName = "@",
                Content = file.Name,
                IsChecked = first,
                Tag = file
            });

            first = false;
        }

        ContentDialog dialog = new()
        {
            Title = $"Install {mod.Name}?",
            Content = panel,
            SecondaryButtonText = "No",
            PrimaryButtonText = "Yes"
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            if (panel.Children.FirstOrDefault(x => x is RadioButton radioButton && radioButton.IsChecked == true)?.Tag is GameBananaFile file)
            {
                AppStatus.Set($"Installing '{file.Name}'", "fa-solid fa-download", isWorkingStatus: true);

                try
                {
                    await Task.Run(async () => {
                        ProfileManager.Shared.Mods.Add(
                            await mod.Full.FromFile(file)
                        );
                    });

                    AppStatus.Set("Install Complete!", "fa-regular fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
                }
                catch (Exception ex)
                {
                    App.ToastError(ex);
                    AppStatus.Set("Install Failed!", "fa-regular fa-circle-xmark", isWorkingStatus: false, temporaryStatusTime: 1.5);
                }
            }
        }
    }

    private async void InitLoad()
    {
        await UpdatePage();
    }

    private async Task UpdatePage()
    {
        Feed = await Fetch(Page + 1, SearchArgument, _includeContentRated);
    }

    private async Task<GameBananaFeed> Fetch(int page, string search, bool includeContentRated = false)
    {
        string endpoint = !string.IsNullOrEmpty(search) && search.Length > 2
            ? string.Format(FEED_ENDPOINT_SEARCH, page, search)
            : string.Format(FEED_ENDPOINT, page);

        using Stream stream = await GameBananaHelper.Get(endpoint);
        GameBananaFeed feed = JsonSerializer.Deserialize<GameBananaFeed>(stream)
            ?? throw new InvalidOperationException($"Could not parse feed from '{FEED_ENDPOINT}'");

        await Task.WhenAll(feed.Records.Select(x => x.DownloadMod()));
        feed.Records = new ObservableCollection<GameBananaModInfo>(
            feed.Records.Where(x =>
                !x.Full.IsTrashed &&
                !x.Full.IsFlagged &&
                !x.IsObsolete &&
                (_includeContentRated || !x.IsContentRated) &&
                !x.Full.IsPrivate
            )
        );

        _ = Task.Run(() => DownloadThumbnails(feed));
        return feed;
    }

    private static async Task DownloadThumbnails(GameBananaFeed feed)
    {
        foreach (var mod in feed.Records)
        {
            if (mod.Media.Images.FirstOrDefault() is GameBananaImage img)
            {
                byte[] image = await _client
                    .GetByteArrayAsync($"{img.BaseUrl}/{img.SmallFile}");
                using MemoryStream ms = new(image);
                mod.Thumbnail = new Bitmap(ms);
            }
        }
    }
}


