using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Tkmm.Core.Components;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Models.Mods;

namespace Tkmm.ViewModels.Pages;

public partial class ProfilesPageViewModel : ObservableObject
{
    public ProfileMod? Selected {
        get => ProfileManager.Shared.Current.Selected;
        set {
            OnPropertyChanging(nameof(Selected));
            ProfileManager.Shared.Current.Selected = value;
            OnPropertyChanged(nameof(Selected));
        }
    }

    [ObservableProperty]
    private ObservableCollection<Mod> _filteredMods = GetOrderedMods();

    [ObservableProperty]
    private string? _filterArgument;

    [ObservableProperty]
    private Mod? _masterSelected;

    public ProfilesPageViewModel()
    {
        ProfileManager.Shared.Current.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(ProfileManager.Shared.Current.Selected)) {
                Selected = ProfileManager.Shared.Current.Selected;
            }
        };
    }

    [RelayCommand]
    private void Remove()
    {
        if (Selected is not null) {
            ProfileManager.Shared.Current.Mods.Remove(Selected);
        }
    }

    [RelayCommand]
    private Task MoveUp()
    {
        if (Selected is not null) {
            Selected = ProfileManager.Shared.Current.Move(Selected, -1);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task MoveDown()
    {
        if (Selected is not null) {
            Selected = ProfileManager.Shared.Current.Move(Selected, 1);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private static async Task Uninstall(Mod? target)
    {
        if (target is null) {
            return;
        }

        ContentDialog dialog = new() {
            Content = $"""
            Are you sure you would like to uninstall '{target.Name}'?

            This cannot be undone.
            """,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = "Uninstall Mod",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
            Title = "Warning"
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary) {
            target.Uninstall();
        }
    }

    [RelayCommand]
    private static async Task DeleteCurrentProfile()
    {
        if (ProfileManager.Shared.Profiles.Count < 2) {
            App.Toast("Cannot delete the last profile!", "Error", NotificationType.Error);
            return;
        }

        ContentDialog dialog = new() {
            Title = "Delete Profile",
            Content = $"""
            Are you sure you would like to delete the profile '{ProfileManager.Shared.Current.Name}'?

            This cannot be undone.
            """,
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary) {
            return;
        }

        int currentIndex = ProfileManager.Shared.Profiles.IndexOf(ProfileManager.Shared.Current);
        ProfileManager.Shared.Profiles.RemoveAt(currentIndex);
        ProfileManager.Shared.Current = ProfileManager.Shared.Profiles[currentIndex == ProfileManager.Shared.Profiles.Count
            ? --currentIndex : currentIndex
        ];
    }

    partial void OnFilterArgumentChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) {
            FilteredMods = GetOrderedMods();
            return;
        }

        FilteredMods = [..ProfileManager.Shared.Mods
            .Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase) || value.Contains(x.Name, StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(x => x.Name)
        ];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ObservableCollection<Mod> GetOrderedMods()
    {
        return [.. ProfileManager.Shared.Mods.OrderBy(x => x.Name)];
    }
}
