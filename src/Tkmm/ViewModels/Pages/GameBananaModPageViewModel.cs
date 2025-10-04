using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Tkmm.Actions;
using TkSharp.Core;
using TkSharp.Core.Models;
using TkSharp.Extensions.GameBanana;
using TkSharp.Extensions.GameBanana.Helpers;

namespace Tkmm.ViewModels.Pages;

public partial class GameBananaModPageViewModel : ObservableObject
{
    [ObservableProperty]
    private GameBananaMod? _selectedMod;

    [ObservableProperty]
    private int _selectedImageIndex;

    [ObservableProperty]
    private ObservableCollection<object> _images = [];

    [ObservableProperty]
    private object? _bananaIcon;

    partial void OnSelectedImageIndexChanged(int value)
    {
        OnPropertyChanged(nameof(SelectedImage));
        OnPropertyChanged(nameof(CanGoToPreviousImage));
        OnPropertyChanged(nameof(CanGoToNextImage));
    }
    
    public string FormattedDateAdded => SelectedMod?.DateAdded > 0 
        ? DateTimeOffset.FromUnixTimeSeconds(SelectedMod.DateAdded).ToString("MMM d, yyyy")
        : string.Empty;

    public string FormattedDateUpdated => SelectedMod?.DateUpdated > 0 
        ? DateTimeOffset.FromUnixTimeSeconds(SelectedMod.DateUpdated).ToString("MMM d, yyyy")
        : string.Empty;

    public bool HasImages => Images.Count > 0;

    public bool ShowNoImagesMessage => Images.Count == 0;

    public bool ShowNoThumbnailMessage => Images.Count == 0;

    public bool CanGoToPreviousImage => Images.Count > 1 && SelectedImageIndex > 0;

    public bool CanGoToNextImage => Images.Count > 1 && SelectedImageIndex < Images.Count - 1;

    public object? SelectedImage => Images.Count > 0 && SelectedImageIndex >= 0 && SelectedImageIndex < Images.Count 
        ? Images[SelectedImageIndex] 
        : null;

    public string ModUrl => SelectedMod != null ? $"https://gamebanana.com/mods/{SelectedMod.Id}" : string.Empty;

    public static GameBananaModPageViewModel CreateForMod(GameBananaMod mod)
    {
        var viewer = new GameBananaModPageViewModel();
        viewer.LoadMod(mod);
        return viewer;
    }

    private void LoadMod(GameBananaMod mod)
    {
        SelectedMod = mod;
        
        OnPropertyChanged(nameof(SelectedMod));
        OnPropertyChanged(nameof(ModUrl));
        OnPropertyChanged(nameof(FormattedDateAdded));
        OnPropertyChanged(nameof(FormattedDateUpdated));
        
        _ = Task.Run(async () => {
            await Task.Delay(100);
            await LoadBananaIconAsync();
            await LoadAvatarsAsync();
            await LoadImagesAsync();
        });
    }

    public void Dispose()
    {
        SelectedMod = null;
        Images.Clear();
        SelectedImageIndex = 0;
        BananaIcon = null;
        
        if (SelectedMod?.Credits is null) {
            return;
        }
        foreach (var author in SelectedMod.Credits.SelectMany(creditGroup => creditGroup.Authors)) {
            author.LoadedAvatar = null;
        }
    }

    [RelayCommand]
    private void NextImage()
    {
        if (Images.Count > 1)
        {
            SelectedImageIndex = (SelectedImageIndex + 1) % Images.Count;
            OnPropertyChanged(nameof(SelectedImage));
        }
    }

    [RelayCommand]
    private void PreviousImage()
    {
        if (Images.Count > 1)
        {
            SelectedImageIndex = SelectedImageIndex == 0 ? Images.Count - 1 : SelectedImageIndex - 1;
            OnPropertyChanged(nameof(SelectedImage));
        }
    }

    [RelayCommand]
    private void SelectImage(object? image)
    {
        if (image == null || !Images.Contains(image)) {
            return;
        }
        SelectedImageIndex = Images.IndexOf(image);
        OnPropertyChanged(nameof(SelectedImage));
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

    private async Task LoadBananaIconAsync()
    {
        var bitmap = await LoadImageFromUrlAsync("https://images.gamebanana.com/static/img/banana.png", "banana icon");
        
        if (bitmap != null) {
            await Dispatcher.UIThread.InvokeAsync(() => {
                BananaIcon = bitmap;
            });
        }
    }

    private async Task LoadAvatarsAsync()
    {
        if (SelectedMod?.Credits is not null) {
            foreach (var author in SelectedMod.Credits.SelectMany(creditGroup => creditGroup.Authors)) {
                await LoadSingleAvatarAsync(author);
            }
        }
    }

    private async Task LoadSingleAvatarAsync(GameBananaAuthor author)
    {
        var avatarUrl = !string.IsNullOrEmpty(author.AvatarUrl) 
            ? author.AvatarUrl 
            : "https://images.gamebanana.com/static/img/defaults/avatar.gif";

        var bitmap = await LoadImageFromUrlAsync(avatarUrl, $"avatar for {author.Name}");
        if (bitmap != null)
        {
            await Dispatcher.UIThread.InvokeAsync(() => {
                author.LoadedAvatar = bitmap;
            });
        }
    }

    private async Task LoadImagesAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() => {
            Images.Clear();
            SelectedImageIndex = 0;
            OnPropertyChanged(nameof(SelectedImageIndex));
            OnPropertyChanged(nameof(HasImages));
            OnPropertyChanged(nameof(ShowNoImagesMessage));
            OnPropertyChanged(nameof(ShowNoThumbnailMessage));
        });
        
        if (SelectedMod?.Media?.Images is not null)
        {
            foreach (var image in SelectedMod.Media.Images)
            {
                var bitmap = await LoadImageFromUrlAsync($"{image.BaseUrl}/{image.File}", "mod image");
                if (bitmap != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => {
                        Images.Add(bitmap);
                        OnPropertyChanged(nameof(HasImages));
                        OnPropertyChanged(nameof(ShowNoImagesMessage));
                        OnPropertyChanged(nameof(ShowNoThumbnailMessage));
                        OnPropertyChanged(nameof(CanGoToNextImage));
                        OnPropertyChanged(nameof(CanGoToPreviousImage));
                        
                        if (Images.Count == 1) {
                            OnPropertyChanged(nameof(SelectedImageIndex));
                            OnPropertyChanged(nameof(SelectedImage));
                        }
                    });
                }
            }
        }
    }

    private async Task<object?> LoadImageFromUrlAsync(string url, string description)
    {
        try
        {
            await using Stream imageStream = await DownloadHelper.Client.GetStreamAsync(url);
            await using MemoryStream ms = new();
            await imageStream.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            if (TkThumbnail.CreateBitmap is not null)
            {
                return TkThumbnail.CreateBitmap(ms);
            }
        }
        catch (Exception ex)
        {
            TkLog.Instance.LogWarning("Failed to load {Description}: {Message}", description, ex.Message);
        }
        
        return null;
    }
}
