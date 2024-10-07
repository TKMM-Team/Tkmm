using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Builders;
using Tkmm.Core;
using Tkmm.Core.Abstractions;
using Tkmm.Dialogs;

namespace Tkmm.Actions;

public sealed partial class MergeActions : GuardedActionGroup<MergeActions>
{
    protected override string ActionGroupName { get; } = nameof(MergeActions).Humanize();
    
    [RelayCommand]
    public async Task Merge(CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }
        
        // TODO: Find a better way to store conditional popups
        if (!Config.Shared.ExportLocations.Any(x => x.IsEnabled) && !Config.Shared.SuppressExportLocationsPrompt) {
            ContentDialog dialog = new() {
                Title = "Warning",
                Content = new StackPanel {
                    Spacing = 5,
                    Children = {
                        new TextBlock {
                            Text = "There are currently no export locations enabled. Would you like to configure them now?",
                            TextWrapping = TextWrapping.WrapWithOverflow
                        },
                        new CheckBox {
                            Content = "Never show again (recomended for Switch users).",
                            DataContext = Config.Shared,
                            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(Config.SuppressExportLocationsPrompt))
                        }
                    }
                },
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            // TODO: Abstract to another location (EditExportLocations)
            {
                if (await dialog.ShowAsync() is ContentDialogResult.Primary) {
                    await ExportLocationControlBuilder.Edit(Config.Shared.ExportLocations);
                }
                Config.Shared.Save();
            }
        }

        try {
            // TODO: Start the dumb trivia thing (if enabled)
            await TKMM.ModManager.Merge(ct);
            App.Toast($"The profile '{TKMM.ModManager.CurrentProfile.Name}' was merged successfully.",
                "Merge Successful!", NotificationType.Success, TimeSpan.FromDays(5));
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured when merging the selected profile '{Profile}'.",
                TKMM.ModManager.CurrentProfile.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    } 
    
    [RelayCommand]
    public async Task MergeProfile(ITkProfile profile, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        try {
            await TKMM.ModManager.Merge(profile, ct);
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured when merging the profile '{Profile}'.", profile.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }
}