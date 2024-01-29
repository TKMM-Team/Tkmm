using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using System.Collections;
using Tkmm.Core.Components;
using Tkmm.Core.Models.Mods;
using Tkmm.Helpers;

namespace Tkmm.ViewModels.Pages;

public partial class HomePageViewModel : ObservableObject
{
    [ObservableProperty]
    private Mod? _currentMod;

    [ObservableProperty]
    private bool _isSelected = false;

    [RelayCommand]
    private async Task ShowContributors()
    {
        ContentDialog dialog = new() {
            Title = "Contributors",
            Content = new TextBlock {
                Text = $"""
                {string.Join("\n", CurrentMod?.Contributors
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
        await ModManager.Shared.Merge();
    }

    [RelayCommand]
    private Task MoveUp()
    {
        Move(-1);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task MoveDown()
    {
        Move(1);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task Remove()
    {
        if (CurrentMod is null) {
            return Task.CompletedTask;
        }

        int removeIndex = ModManager.Shared.Mods.IndexOf(CurrentMod);
        ModManager.Shared.Mods.RemoveAt(removeIndex);

        if (ModManager.Shared.Mods.Count == 0) {
            return Task.CompletedTask;
        }

        while (removeIndex >= ModManager.Shared.Mods.Count) {
            removeIndex--;
        }

        CurrentMod = ModManager.Shared.Mods[removeIndex];
        return Task.CompletedTask;
    }

    private void Move(int offset)
    {
        if (CurrentMod is null) {
            return;
        }

        int currentIndex = ModManager.Shared.Mods.IndexOf(CurrentMod);
        int newIndex = currentIndex + offset;

        if (newIndex < 0 || newIndex >= ModManager.Shared.Mods.Count) {
            return;
        }

        Mod store = ModManager.Shared.Mods[newIndex];
        ModManager.Shared.Mods[newIndex] = CurrentMod;
        ModManager.Shared.Mods[currentIndex] = store;

        CurrentMod = ModManager.Shared.Mods[newIndex];
    }

    public HomePageViewModel()
    {
        ModManager.Shared.Mods.CollectionChanged += ModsUpdated;
        CurrentMod = ModManager.Shared.Mods.FirstOrDefault();
    }

    private async void ModsUpdated(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is IList mods) {
            foreach (Mod mod in mods) {
                await ModHelper.ResolveThumbnail(mod);

                if (!ModManager.Shared.Mods.Contains(mod)) {
                    CurrentMod = mod;
                }
            }
        }
    }

    partial void OnCurrentModChanged(Mod? value)
    {
        IsSelected = value is not null;
    }
}
