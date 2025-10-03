using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Tkmm.Actions;
using Tkmm.Views.Common;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.ViewModels.Pages;

public partial class GameBananaPageViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isShowingDetail;

    [ObservableProperty]
    private double _viewerOpacity;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private double? _loadProgress;

    [ObservableProperty]
    private double? _downloadSpeed;

    public GameBananaModBrowserViewModel Browser { get; } = new();
    public GameBananaModPageViewModel? Viewer { get; private set; }

    public GameBananaPageViewModel()
    {
        Browser.PropertyChanged += (_, e) => {
            switch (e.PropertyName) {
                case nameof(Browser.IsLoading):
                    IsLoading = Browser.IsLoading;
                    break;
                case nameof(Browser.LoadProgress):
                    LoadProgress = Browser.LoadProgress;
                    break;
                case nameof(Browser.DownloadSpeed):
                    DownloadSpeed = Browser.DownloadSpeed;
                    break;
            }
        };
    }

    [RelayCommand]
    private async Task ViewMod(GameBananaModRecord mod)
    {
        if (mod.Full is null) {
            await mod.DownloadFullMod();
        }

        if (mod.Full is null) {
            return;
        }

        Viewer?.Dispose();
        Viewer = GameBananaModPageViewModel.CreateForMod(mod.Full);
        OnPropertyChanged(nameof(Viewer));
        IsShowingDetail = true;
        ViewerOpacity = 1.0;
    }

    [RelayCommand]
    private void BackToMain()
    {
        ViewerOpacity = 0.0;
        Task.Delay(300).ContinueWith(_ => {
            Dispatcher.UIThread.Post(() => {
                Viewer?.Dispose();
                Viewer = null;
                OnPropertyChanged(nameof(Viewer));
                IsShowingDetail = false;
            });
        });
    }

    [RelayCommand]
    private static async Task InstallMod(GameBananaModRecord mod)
    {
        if (mod.Full is null) {
            await mod.DownloadFullMod();
        }

        GameBananaInstallPreview preview = new() {
            DataContext = mod
        };

        GameBananaFile? target = mod.Full?.Files
            .FirstOrDefault(file => file.IsSelected);

        ContentDialog dialog = new() {
            Title = $"Install {mod.Name}",
            Content = preview,
            SecondaryButtonText = "Cancel",
            PrimaryButtonText = "Install",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = target is not null,
        };

        foreach (GameBananaFile file in mod.Full!.Files) {
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

}