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
using Tkmm.Helpers;
using Tkmm.Models;
using TotkCommon;

namespace Tkmm.Actions;

public sealed partial class MergeActions : GuardedActionGroup<MergeActions>
{
    protected override string ActionGroupName { get; } = nameof(MergeActions).Humanize();

    [RelayCommand]
    public Task Merge(CancellationToken ct = default)
    {
        return Merge(TKMM.ModManager.CurrentProfile, ct);
    }

    public async Task Merge(ITkProfile profile, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        // TODO: Find a better way to store conditional popups
        const string suppressMergeExportLocationsPrompt = "SuppressMergeExportLocationsPrompt";
        if (!Config.Shared.ExportLocations.Any(exportLocation => exportLocation.IsEnabled) && !NamedDialogConfig.Shared[suppressMergeExportLocationsPrompt].IsSuppressed) {
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
                            DataContext = NamedDialogConfig.Shared,
                            [!ToggleButton.IsCheckedProperty] = NamedDialogConfig.GetBinding(suppressMergeExportLocationsPrompt)
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
            await TKMM.ModManager.Merge(profile, ct);
            App.Toast($"The profile '{profile.Name}' was merged successfully.",
                "Merge Successful!", NotificationType.Success, TimeSpan.FromDays(5));
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured when merging the selected profile '{Profile}'.",
                profile.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public Task ExportToSdCard(CancellationToken ct = default)
    {
        return ExportToSdCard(TKMM.ModManager.CurrentProfile, ct);
    }

    public async Task ExportToSdCard(ITkProfile profile, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        DisplayDisk[] disks = DriveInfo.GetDrives()
            .Where(static driveInfo => {
                try {
                    return driveInfo is {
                        DriveType: DriveType.Removable,
                        DriveFormat: "FAT32"
                    };
                }
                catch {
                    return false;
                }
            })
            .Select(static driveInfo => new DisplayDisk(driveInfo))
            .ToArray();

        if (disks.Length is 0) {
            await ErrorDialog.ShowAsync(
                new DriveNotFoundException("No suitable disks found. Please make sure you have an SD card inserted or connected virtually over USB.")
            );

            return;
        }

        ContentDialog dialog = new() {
            Title = "Select an SD card",
            Content = new ComboBox {
                ItemsSource = disks,
                SelectedIndex = 0,
                DisplayMemberBinding = new Binding("DisplayName")
            },
            PrimaryButtonText = "Merge and export",
            SecondaryButtonText = "Cancel"
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary || dialog.Content is not ComboBox {
                SelectedItem: DisplayDisk {
                    Drive: DriveInfo drive
                }
            }) return;

        await Merge(profile, ct);

        try {
            string output = Path.Combine(drive.Name, "atmosphere", "contents", Totk.TITLE_ID);
            
            // TODO: Ask before deleting
            // TODO: Abstract romfs, exefs folder names to constants
            DirectoryHelper.DeleteTargetsFromDirectory(output, ["romfs", "exefs"], recursive: true);
            
            // TODO: Who decides the merge output location? (Should be an interface because it changes on TKMM-OS)
            DirectoryHelper.Copy(".merge", output, overwrite: true);
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured when exporting the profile '{Profile}' to the external drive " +
                                     "'{TargetDrive}'.", profile.Name, drive.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }
}