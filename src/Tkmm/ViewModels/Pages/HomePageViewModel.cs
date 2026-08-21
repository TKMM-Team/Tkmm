#if RELEASE
using Avalonia.Threading;
#endif
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Tkmm.Actions;
using Tkmm.Core;
using Tkmm.Models;
using TkSharp;

namespace Tkmm.ViewModels.Pages;

public partial class HomePageViewModel : ObservableObject
{
    public static LayoutConfig Layout { get; } = LayoutConfig.Load("HomePageLayout");

    public static TkModManager ModManager => TKMM.ModManager;

    public Config Config => Config.Shared;

#if SWITCH
    public bool IsHybridMode => false;
#else
    public bool IsHybridMode => Config.TkmmMode.IsHybrid;
#endif

    public bool ShowApplyButton => !IsHybridMode;

    public bool ShowApplyDropDown => IsHybridMode;

    public HomePageViewModel()
    {
#if RELEASE
        _ = Dispatcher.UIThread.InvokeAsync(async () => await SystemActions.CheckForUpdates(isUserInvoked: false));
#endif
#if !SWITCH
        Config.PropertyChanged += (_, e) => {
            if (e.PropertyName is not nameof(Config.TkmmMode)) {
                return;
            }

            OnPropertyChanged(nameof(IsHybridMode));
            OnPropertyChanged(nameof(ShowApplyButton));
            OnPropertyChanged(nameof(ShowApplyDropDown));
        };
#endif
    }

    [RelayCommand]
    private static async Task ShowContributors()
    {
        ContentDialog dialog = new() {
            Title = "Contributors",
            Content = new TextBlock {
                Text = string.Join("\n", TKMM.ModManager.GetCurrentProfile().Selected?.Mod.Contributors
                    .Select(contributor => $"{contributor.Author}: {contributor.Contribution}") ?? []
                ),
                TextWrapping = TextWrapping.WrapWithOverflow
            },
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "Dismiss"
        };

        await dialog.ShowAsync();
    }

    [RelayCommand]
    private static Task Apply()
    {
#if SWITCH
        return MergeActions.Instance.Merge();
#else
        return Config.Shared.TkmmMode.IsSwitch
            ? MergeActions.Instance.ExportToSdCard()
            : MergeActions.Instance.Merge();
#endif
    }

    [RelayCommand]
    private static Task ApplyToEmulator()
    {
        return MergeActions.Instance.Merge();
    }

    [RelayCommand]
    private static Task ApplyToSdCard()
    {
#if SWITCH
        return Task.CompletedTask;
#else
        return MergeActions.Instance.ExportToSdCard();
#endif
    }

    [RelayCommand]
    private static Task Install()
    {
        return ImportActions.Instance.ImportFromFile();
    }

    [RelayCommand]
    private static Task Update()
    {
        return ImportActions.Instance.Update();
    }

    [RelayCommand]
    private static void MoveUp()
    {
        TKMM.ModManager.GetCurrentProfile().MoveUp();
    }

    [RelayCommand]
    private static void MoveDown()
    {
        TKMM.ModManager.GetCurrentProfile().MoveDown();
    }

    [RelayCommand]
    private static void MoveToTop()
    {
        TKMM.ModManager.GetCurrentProfile().MoveToTop();
    }

    [RelayCommand]
    private static void MoveToBottom()
    {
        TKMM.ModManager.GetCurrentProfile().MoveToBottom();
    }

    [RelayCommand]
    private static Task Remove()
    {
        return ModActions.Instance.RemoveModFromProfile();
    }
}
