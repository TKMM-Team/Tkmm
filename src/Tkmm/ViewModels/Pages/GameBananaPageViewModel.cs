using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
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
                    UpdateCombinedLoading();
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

    private void UpdateCombinedLoading()
    {
        IsLoading = Browser.IsLoading || (Viewer?.IsLoading ?? false);
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
        Viewer.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(GameBananaModPageViewModel.IsLoading)) {
                UpdateCombinedLoading();
            }
        };
        UpdateCombinedLoading();
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
                UpdateCombinedLoading();
            });
        });
    }

    public async Task OpenModInViewerAsync(long modId)
    {
        try {
            TkStatus.Set("Loading mod", TkIcons.GEAR_FOLDER, StatusType.Working);
            
            var modRecord = new GameBananaModRecord { Id = (int)modId };
            await modRecord.DownloadFullMod();
            
            if (modRecord.Full == null) {
                TkStatus.SetTemporary("Failed to load mod", TkIcons.ERROR);
                return;
            }

            if (modRecord.Full.Game.Id != 7617) {
                TkStatus.SetTemporary("This mod is not for TOTK", TkIcons.ERROR);
                return;
            }

            Viewer?.Dispose();
            Viewer = GameBananaModPageViewModel.CreateForMod(modRecord.Full);
            Viewer.PropertyChanged += (_, e) => {
                if (e.PropertyName == nameof(GameBananaModPageViewModel.IsLoading)) {
                    UpdateCombinedLoading();
                }
            };
            UpdateCombinedLoading();
            OnPropertyChanged(nameof(Viewer));
            IsShowingDetail = true;
            ViewerOpacity = 1.0;
            
            TkStatus.SetTemporary("Mod loaded successfully", TkIcons.CIRCLE_CHECK);
        }
        catch (Exception ex) {
            TkStatus.SetTemporary("Error loading mod", TkIcons.ERROR);
            TkLog.Instance.LogError(ex, "An error occurred while loading mod {ModId}.", modId);
        }
    }

    [RelayCommand]
    private static async Task InstallMod(GameBananaModRecord mod)
    {
        if (mod.Full is null) {
            await mod.DownloadFullMod();
        }

        var target = mod.Full?.Files
            .FirstOrDefault(file => file.IsSelected);

        if (target is null) {
            return;
        }

        TkStatus.Set($"Downloading '{target.Name}'", "fa-solid fa-download", StatusType.Working);
        await ModActions.Instance.Install((mod.Full, target));
    }

}