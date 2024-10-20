using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Tkmm.Abstractions;
using Tkmm.Core;

namespace Tkmm.Actions;

public sealed class ProfileActions : GuardedActionGroup<ProfileActions>
{
    protected override string ActionGroupName { get; } = nameof(ProfileActions).Humanize();

    public Task DeleteProfile()
        => DeleteProfile(TKMM.ModManager.CurrentProfile ?? TKMM.ModManager.Profiles[0]);
    
    public async Task DeleteProfile(ITkProfile profile)
    {
        await CanActionRun(showError: false);
        
        if (TKMM.ModManager.Profiles.Count is 1) {
            App.Toast("One profile must always exist.", "Cannot delete profile", NotificationType.Warning);
            return;
        }
        
        ContentDialog dialog = new() {
            Title = "Permenently delete profile",
            Content = $"""
                WARNING: THIS CANNOT BE UNDONE
                
                Are you sure you would like to permenently delete the profile '{profile.Name}'?
                """,
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
        };
        
        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return;
        }
        
        int removeIndex = TKMM.ModManager.Profiles.IndexOf(profile);
        TKMM.ModManager.Profiles.RemoveAt(removeIndex);
        
        while (removeIndex >= TKMM.ModManager.Profiles.Count) {
            removeIndex--;
        }

        TKMM.ModManager.CurrentProfile = TKMM.ModManager.Profiles[removeIndex];
    }
}