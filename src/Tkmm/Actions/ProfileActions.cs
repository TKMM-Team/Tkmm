using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Tkmm.Core;
using TkSharp.Core.Models;

namespace Tkmm.Actions;

public sealed class ProfileActions : GuardedActionGroup<ProfileActions>
{
    protected override string ActionGroupName { get; } = nameof(ProfileActions).Humanize();

    public Task DeleteProfile()
        => DeleteProfile(TKMM.ModManager.GetCurrentProfile());
    
    private async Task DeleteProfile(TkProfile profile)
    {
        await CanActionRun(showError: false);
        
        if (TKMM.ModManager.Profiles.Count is 1) {
            App.Toast(Locale["Profile_OneProfileMustExist"], Locale["Profile_CannotDeleteProfile"], NotificationType.Warning);
            return;
        }
        
        ContentDialog dialog = new() {
            Title = Locale["Profile_PermanentlyDeleteProfileTitle"],
            Content = string.Format(Locale["Profile_PermanentlyDeleteProfileDescription"], profile.Name),
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
        };
        
        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return;
        }
        
        var removeIndex = TKMM.ModManager.Profiles.IndexOf(profile);
        TKMM.ModManager.Profiles.RemoveAt(removeIndex);
        
        while (removeIndex >= TKMM.ModManager.Profiles.Count) {
            removeIndex--;
        }

        TKMM.ModManager.CurrentProfile = TKMM.ModManager.Profiles[removeIndex];
    }
}