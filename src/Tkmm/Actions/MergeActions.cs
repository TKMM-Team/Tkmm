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
        return Merge(TKMM.ModManager.GetCurrentProfile(), ipsOutputPath: null, ct);
    }

    public Task Merge(string ipsOutputPath, CancellationToken ct = default)
    {
        return Merge(TKMM.ModManager.GetCurrentProfile(), ipsOutputPath, ct);
    }

    public Task Merge(TkProfile profile, CancellationToken ct = default)
    {
        return Merge(profile, ipsOutputPath: null, ct);
    }

    public async Task Merge(TkProfile profile, string? ipsOutputPath = null, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        CancellationTokenSource modalCancelTokenSource = new();

        try {
            var drive = Path.GetPathRoot(TKMM.MergedOutputFolder);
            if (!string.IsNullOrEmpty(drive) && !Directory.Exists(drive)) {
                throw new DirectoryNotFoundException(
                    $"The path {TKMM.MergedOutputFolder} could not be used because its root {drive} does not exist."
                );
            }

            TkStatus.Set("Merging", "fa-code-merge", StatusType.Working);
            MergingModal.ShowModal(modalCancelTokenSource.Token);
            await TKMM.Merge(profile, ipsOutputPath, ct: modalCancelTokenSource.Token);
            App.Toast(string.Format(Locale["MergeActions_MergeSuccessful"], profile.Name),
                Locale["MergeActions_MergeSuccessfulTitle"], NotificationType.Success, TimeSpan.FromDays(5));
            TkStatus.SetTemporary("Merge completed", "fa-circle-check");
        }
        catch (Exception ex) {
            TkStatus.SetTemporary("Merge failed", "fa-circle-exclamation");
            TkLog.Instance.LogError(ex, string.Format(Locale["MergeActions_ErrorMergingProfile"], profile.Name));
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

    private async Task ExportToSdCard(TkProfile profile, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        var disks = DriveInfo.GetDrives()
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
                new DriveNotFoundException(Locale["MergeActions_NoSuitableDisks"])
            );

            return;
        }

        ContentDialog dialog = new() {
            Title = Locale["MergeActions_SelectSdCard"],
            Content = new ComboBox {
                ItemsSource = disks,
                SelectedIndex = 0,
                DisplayMemberBinding = new Binding("DisplayName")
            },
            PrimaryButtonText = Locale["MergeActions_MergeAndExport"],
            SecondaryButtonText = Locale["Action_Cancel"]
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary || dialog.Content is not ComboBox {
                SelectedItem: DisplayDisk {
                    Drive: { } drive
                }
            }) return;
        
        var output = Path.Combine(drive.Name, "atmosphere", "contents", "0100F2C0115B6000");
        var ipsOutputPath = Path.Combine(drive.Name, "atmosphere", "exefs_patches", "TKMM");
        await Merge(profile, ipsOutputPath, ct);

        try {
            var canDeleteResult = await MessageDialog.Show(
                Locale["MergeActions_DeleteAtmosphereContents"], Locale["Action_Warning"], MessageDialogButtons.YesNoCancel);
            
            if (canDeleteResult is not MessageDialogResult.Yes) {
                return;
            }
            
            TKMM.EmptyMergeOutput(output);
            DirectoryHelper.Copy(TKMM.MergedOutputFolder, output, overwrite: true);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, string.Format(Locale["MergeActions_ErrorExportingProfile"], profile.Name, drive.Name));
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
            TkLog.Instance.LogError(ex, Locale["MergeActions_ErrorOpeningFolder"]);
            await ErrorDialog.ShowAsync(ex);
        }
    }
}