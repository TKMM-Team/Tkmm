using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Tkmm.Core.Components;

namespace Tkmm.ViewModels.Pages;

public partial class ProfilesPageViewModel : ObservableObject
{
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
            ? --currentIndex : ++currentIndex
        ];
    }
}
