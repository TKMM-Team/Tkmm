using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using System.Collections;
using Tkmm.Builders.MenuModels;
using Tkmm.Core.Components;
using Tkmm.Core.Components.Models;
using Tkmm.Helpers;
using Tkmm.Models;

namespace Tkmm.ViewModels.Pages;

public partial class HomePageViewModel : ObservableObject
{
    public static LayoutConfig Layout { get; } = LayoutConfig.Load("HomePageLayout");

    public ProfileMod? Current {
        get => ProfileManager.Shared.Current.Selected;
        set {
            OnPropertyChanging();
            ProfileManager.Shared.Current.Selected = value;
            OnPropertyChanged();

            if (value?.Mod is null) {
                return;
            }

            // Re-validate the description
            string content = value.Mod.Description;
            value.Mod.Description = string.Empty;
            value.Mod.Description = content;
        }
    }

    [RelayCommand]
    private async Task ShowContributors()
    {
        ContentDialog dialog = new() {
            Title = "Contributors",
            Content = new TextBlock {
                Text = $"""
                {string.Join("\n", Current?.Mod?.Contributors
                    .Select(x => $"{x.Name}: {string.Join(", ", x.Contributions)}") ?? [])}
                """,
                TextWrapping = TextWrapping.WrapWithOverflow
            },
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "Dismiss"
        };

        await dialog.ShowAsync();
    }

    [RelayCommand]
    private static async Task Merge()
    {
        await MergerOperations.Merge();
    }

    [RelayCommand]
    private static async Task Install()
    {
        await ShellViewMenu.ImportModFile();
    }

    [RelayCommand]
    private Task MoveUp()
    {
        if (Current is not null) {
            Current = ProfileManager.Shared.Current.Move(Current, -1);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task MoveDown()
    {
        if (Current is not null) {
            Current = ProfileManager.Shared.Current.Move(Current, 1);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task Remove()
    {
        if (Current is null) {
            return Task.CompletedTask;
        }

        int removeIndex = ProfileManager.Shared.Current.Mods.IndexOf(Current);
        ProfileManager.Shared.Current.Mods.RemoveAt(removeIndex);

        if (ProfileManager.Shared.Current.Mods.Count == 0) {
            return Task.CompletedTask;
        }

        while (removeIndex >= ProfileManager.Shared.Current.Mods.Count) {
            removeIndex--;
        }

        Current = ProfileManager.Shared.Current.Mods[removeIndex];
        return Task.CompletedTask;
    }

    public HomePageViewModel()
    {
        ProfileManager.Shared.Current.Mods.CollectionChanged += ModsUpdated;
        ProfileManager.Shared.Current.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(ProfileManager.Shared.Current.Selected)) {
                Current = ProfileManager.Shared.Current.Selected;
            }
        };

        _ = Task.Run(async () => {
            await Task.Delay(TimeSpan.FromSeconds(5));

            (bool hasUpdate, string tag) = await AppManager.HasUpdate();
            if (hasUpdate) {
                App.Toast($"TKMM {tag} is available! (Click here to install)", "Update Available",
                    // ReSharper disable once AsyncVoidLambda
                    NotificationType.Information, TimeSpan.FromSeconds(10), async () => {
                        await App.PromptUpdate();
                    });
            }
        });
    }

    private async void ModsUpdated(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not IList mods) {
            return;
        }

        foreach (ProfileMod profileMod in mods) {
            if (profileMod.Mod is null) {
                continue;
            }

            await ModHelper.ResolveThumbnail(profileMod.Mod);

            if (!ProfileManager.Shared.Current.Mods.Contains(profileMod)) {
                Current = profileMod;
            }
        }
    }
}
