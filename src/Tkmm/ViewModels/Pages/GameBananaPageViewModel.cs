using System.Diagnostics;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Tkmm.Actions;
using Tkmm.Components;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.ViewModels.Pages;

public partial class GameBananaPageViewModel : ObservableObject
{
    private const string BANANA_LOGO_URL = "https://images.gamebanana.com/static/img/banana.png";

    private static Task<object?>? _bananaIconTask;

    public static object? Icon { get; private set; }

    public object? BananaIcon => Icon;

    public static Task<object?> LoadBananaIconAsync(CancellationToken ct = default)
    {
        if (Icon is not null) {
            return Task.FromResult<object?>(Icon);
        }

        return _bananaIconTask ??= LoadBananaIconCoreAsync(ct);
    }

    private static async Task<object?> LoadBananaIconCoreAsync(CancellationToken ct)
    {
        try {
            return Icon ??= await TkImageResolver.LoadOrDownloadAsync(BANANA_LOGO_URL, "gamebanana", ct);
        }
        finally {
            _bananaIconTask = null;
        }
    }

    [ObservableProperty]
    public partial bool IsShowingDetail { get; set; }

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
        _ = LoadBananaIconAsync().ContinueWith(
            _ => OnPropertyChanged(nameof(BananaIcon)),
            TaskScheduler.Default);

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

        await ShowViewerAsync(mod.Full);
    }

    private async Task ShowViewerAsync(GameBananaMod mod)
    {
        Viewer?.CancelLoading();
        Viewer?.Reset();
        Viewer = null;
        OnPropertyChanged(nameof(Viewer));

        Viewer = GameBananaModPageViewModel.CreateForMod(mod);
        Viewer.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(GameBananaModPageViewModel.IsLoading)) {
                UpdateCombinedLoading();
            }
        };
        UpdateCombinedLoading();
        OnPropertyChanged(nameof(Viewer));
        IsShowingDetail = true;
        ViewerOpacity = 1.0;

        await Task.Delay(300);

        if (Viewer?.SelectedMod?.Id != mod.Id) {
            return;
        }

        await Viewer.StartLoadingAsync();
    }

    [RelayCommand]
    private void BackToMain()
    {
        ViewerOpacity = 0.0;
        Task.Delay(300).ContinueWith(_ => {
            Dispatcher.UIThread.Post(() => {
                Viewer?.CancelLoading();
                Viewer?.Reset();
                Viewer = null;
                OnPropertyChanged(nameof(Viewer));
                IsShowingDetail = false;
                UpdateCombinedLoading();
            });
        });
    }

    public async Task OpenMemberInBrowserAsync(int memberId)
    {
        if (IsShowingDetail) {
            CloseViewerImmediately();
        }

        await Browser.OpenMemberAsync(memberId);
    }

    private void CloseViewerImmediately()
    {
        Viewer?.CancelLoading();
        Viewer?.Reset();
        Viewer = null;
        OnPropertyChanged(nameof(Viewer));
        IsShowingDetail = false;
        ViewerOpacity = 0;
        UpdateCombinedLoading();
    }

    public async Task OpenModInViewerAsync(long modId, long? fileId = null, bool isSilent = false)
    {
        try {
            var modRecord = new GameBananaModRecord { Id = (int)modId };
            await modRecord.DownloadFullMod();

            if (modRecord.Full == null) {
                TkStatus.SetTemporary(Locale["GameBanana_FailedToLoadMod"], TkIcons.ERROR);
                return;
            }

            if (modRecord.Full.Game.Id != 7617) {
                try {
                    var url = $"https://gamebanana.com/mods/{modId}";
                    _ = Process.Start(new ProcessStartInfo {
                        FileName = url,
                        UseShellExecute = true
                    });
                    TkStatus.SetTemporary("Not a TotK mod, link opened in web browser", TkIcons.CIRCLE_INFO);
                }
                catch {
                    TkStatus.SetTemporary(Locale["GameBanana_FailedToOpenBrowser"], TkIcons.ERROR);
                }

                return;
            }

            if (!isSilent) {
                await ShowViewerAsync(modRecord.Full);
            }

            if (fileId is { } desiredFileId) {
                var target = modRecord.Full.Files.FirstOrDefault(f => f.Id == desiredFileId);
                if (target is null) {
                    TkStatus.SetTemporary(Locale["GameBanana_NoMatchingFile"], TkIcons.ERROR);
                    return;
                }
                
                TkStatus.Set($"Downloading '{target.Name}'", "fa-solid fa-download", StatusType.Working);
                await ModActions.Instance.Install((modRecord.Full, target));
            }
        }
        catch (Exception ex) {
            TkStatus.SetTemporary(Locale["GameBanana_ErrorLoadingMod"], TkIcons.ERROR);
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