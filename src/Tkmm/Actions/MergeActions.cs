using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
using Tkmm.Core.Helpers;
using Tkmm.Dialogs;
using Tkmm.Helpers;
using Tkmm.Models;
using Tkmm.Views.Common;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Actions;

public sealed partial class MergeActions : GuardedActionGroup<MergeActions>
{
    protected override string ActionGroupName { get; } = nameof(MergeActions).Humanize();

    [RelayCommand]
    public Task Merge(CancellationToken ct = default)
    {
        return Merge(TKMM.ModManager.GetCurrentProfile(), ipsOutputPath: null, ct: ct);
    }

    public Task Merge(string ipsOutputPath, CancellationToken ct = default)
    {
        return Merge(TKMM.ModManager.GetCurrentProfile(), ipsOutputPath, ct: ct);
    }

    public Task Merge(TkProfile profile, CancellationToken ct = default)
    {
        return Merge(profile, ipsOutputPath: null, ct: ct);
    }

    public async Task Merge(TkProfile profile, string? ipsOutputPath = null, string? mergeOutput = null, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        CancellationTokenSource modalCancelTokenSource = new();

        try {
#if !SWITCH
            if (!await ExportLocationsHelper.CreateExportLocations()) {
                TkStatus.SetTemporary("Merge Cancelled", "fa-regular fa-ban");
                return;
            }
#endif

            TkStatus.Set("Merging", "fa-code-merge", StatusType.Working);
            MergingModal.ShowModal(modalCancelTokenSource.Token);
            await TKMM.Merge(profile, ipsOutputPath: ipsOutputPath, mergeOutput: mergeOutput, ct: ct);
            App.Toast($"The profile '{profile.Name}' was merged successfully.",
                "Merge Successful!", NotificationType.Success, TimeSpan.FromDays(5));
            TkStatus.SetTemporary("Merge completed", "fa-circle-check");
        }
        catch (Exception ex) {
            await modalCancelTokenSource.CancelAsync();
            
            TkLog.Instance.LogError(ex, "An error occured when merging the selected profile '{Profile}'.",
                profile.Name);
            await ErrorDialog.ShowAsync(ex);
        }
        finally {
            await modalCancelTokenSource.CancelAsync();
        }
    }

    [RelayCommand]
    public Task ExportToSdCard(CancellationToken ct = default)
    {
        return ExportToSdCard(TKMM.ModManager.GetCurrentProfile(), ct);
    }

    public async Task ExportToSdCard(TkProfile profile, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        DisplayDisk[] disks = DriveInfo.GetDrives()
            .Where(static driveInfo => {
                try {
                    return driveInfo is {
                        DriveType: DriveType.Removable
                    } && Directory.Exists(Path.Combine(driveInfo.RootDirectory.FullName, "atmosphere"));
                }
                catch {
                    return false;
                }
            })
            .Select(static driveInfo => new DisplayDisk(driveInfo))
            .ToArray();

        if (disks.Length is 0) {
            await ErrorDialog.ShowAsync(
                new DriveNotFoundException("No suitable disks found. Please make sure you have an SD card inserted or connected virtually over USB and that atmosphere is installed on it.")
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
        
        string output = Path.Combine(drive.Name, "atmosphere", "contents", "0100F2C0115B6000");
        string ipsOutputPath = Path.Combine("..", "..", "exefs_patches", "TKMM");

        try {
            MessageDialogResult canDeleteResult = await MessageDialog.Show(
                "Are you sure you would like to delete the existing atmosphere contents?", "Warning", MessageDialogButtons.YesNoCancel);
            
            if (canDeleteResult is not MessageDialogResult.Yes) {
                return;
            }
            
            await Merge(profile, ipsOutputPath: ipsOutputPath, mergeOutput: output, ct);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured when exporting the profile '{Profile}' to the external drive " +
                                     "'{TargetDrive}'.", profile.Name, drive.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }
    
    [RelayCommand]
    public async Task OpenMergedOutput()
    {
        await CanActionRun(showError: false);

        try {
            ProcessStartInfo info = new() {
                FileName = TKMM.MergedOutputFolder,
                UseShellExecute = true,
                Verb = "open"
            };

            Process.Start(info);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while opening the merged output folder.");
            await ErrorDialog.ShowAsync(ex);
        }
    }
}