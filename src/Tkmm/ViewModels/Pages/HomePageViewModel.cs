using Avalonia.Controls;
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
            OnPropertyChanging(nameof(Current));
            ProfileManager.Shared.Current.Selected = value;
            OnPropertyChanged(nameof(Current));
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
        ProfileManager.Shared.Current.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(ProfileManager.Shared.Current.Selected)) {
                Current = ProfileManager.Shared.Current.Selected;
            }
        };
    }

    private async void ModsUpdated(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is IList mods) {
            foreach (ProfileMod profileMod in mods) {
                if (profileMod.Mod is not null) {
                    await ModHelper.ResolveThumbnail(profileMod.Mod);

                    if (!ProfileManager.Shared.Current.Mods.Contains(profileMod)) {
                        Current = profileMod;
                    }
                }
            }
        }
    }
}
