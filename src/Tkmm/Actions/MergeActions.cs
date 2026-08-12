using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
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
        CancellationTokenSource modalCancelTokenSource = new();

        try {
            TkStatus.Set("Merging", "fa-code-merge", StatusType.Working);
            MergingModal.ShowModal(modalCancelTokenSource.Token);

            await Dispatcher.UIThread.InvokeAsync(static () => { }, DispatcherPriority.Background);

            if (!await CanActionRun()) {
                return;
            }

            var drive = Path.GetPathRoot(TKMM.MergedOutputFolder);
            if (!string.IsNullOrEmpty(drive) && !Directory.Exists(drive)) {
                throw new DirectoryNotFoundException(
                    $"The path {TKMM.MergedOutputFolder} could not be used because its root {drive} does not exist."
                );
            }

            await TKMM.Merge(profile, ipsOutputPath, ct: modalCancelTokenSource.Token);
            App.Toast(string.Format(Locale["MergeActions_MergeSuccessful"], profile.Name),
                Locale["MergeActions_MergeSuccessfulTitle"], NotificationType.Success, TimeSpan.FromDays(5));
            TkStatus.SetTemporary(Locale["Status_MergeCompleted"], "fa-circle-check");
        }
        catch (Exception ex) {
            TkStatus.SetTemporary(Locale["Status_MergeFailed"], "fa-circle-exclamation");
            TkLog.Instance.LogError(ex, string.Format(Locale["MergeActions_ErrorMergingProfile"], profile.Name));
            await ErrorDialog.ShowAsync(ex);
        }
        finally {
            await modalCancelTokenSource.CancelAsync();
        }
    }

#if !SWITCH
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

        var localDisks = await Task.Run(EnumerateLocalExportDisks, ct);
        var mtpDisks = OperatingSystem.IsWindows()
            ? await Task.Run(() => MtpSdCardHelper.FindAtmosphereRoots(TimeSpan.FromSeconds(2))
                .Select(static root => new DisplayDisk(root.DeviceId, root.FriendlyName, root.RootPath))
                .ToArray(), ct)
            : [];

        var disks = localDisks.Concat(mtpDisks).Append(DisplayDisk.ManualSelection).ToArray();

        ContentDialog dialog = new() {
            Title = Locale["MergeActions_SelectSdCard"],
            Content = new ComboBox {
                ItemsSource = disks,
                SelectedIndex = 0,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                MinWidth = 320,
                DisplayMemberBinding = new Binding(nameof(DisplayDisk.DisplayName))
            },
            PrimaryButtonText = Locale["MergeActions_MergeAndExport"],
            SecondaryButtonText = Locale["Action_Cancel"]
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary || dialog.Content is not ComboBox {
                SelectedItem: DisplayDisk selectedDisk
            }) {
            return;
        }

        if (selectedDisk.IsMtp) {
            ContentDialog mtpWarning = new() {
                Title = Locale["Action_Warning"],
                Content = Locale["MergeActions_MtpSlowWarning"],
                PrimaryButtonText = Locale["Action_Continue"],
                CloseButtonText = Locale["Action_Cancel"]
            };

            if (await mtpWarning.ShowAsync() is not ContentDialogResult.Primary) {
                return;
            }
        }

        using var target = await CreateExportTarget(selectedDisk);
        if (target is null) {
            return;
        }

        await Merge(profile, target.LocalIpsDirectory, ct);

        try {
            var canDeleteResult = await MessageDialog.Show(
                Locale["MergeActions_DeleteAtmosphereContents"], Locale["Action_Warning"], MessageDialogButtons.YesNoCancel);
            
            if (canDeleteResult is not MessageDialogResult.Yes) {
                return;
            }

            var progressView = new SdExportProgressView {
                Title = Locale["Menu_ToolsExportToSdCard"]
            };
            progressView.SetIndeterminate(Locale["MergeActions_WipingSdCard"]);
            progressView.Show();
            await Dispatcher.UIThread.InvokeAsync(static () => { }, DispatcherPriority.Background);

            try {
                await Task.Run(() => {
                    var lastUiUpdate = Stopwatch.StartNew();
                    var hasReported = false;
                    var progress = new Progress<(int Copied, int Total)>(report => {
                        if (hasReported && report.Copied != report.Total && lastUiUpdate.ElapsedMilliseconds < 50) {
                            return;
                        }

                        hasReported = true;
                        lastUiUpdate.Restart();

                        Dispatcher.UIThread.Post(() =>
                            progressView.SetProgress(
                                Locale["MergeActions_ExportingToSdCard"],
                                report.Copied,
                                report.Total));
                    });

                    target.Publish(
                        TKMM.MergedOutputFolder,
                        Config.Shared.UseRomfslite,
                        progress,
                        wipeCompleted: () => Dispatcher.UIThread.Post(() =>
                            progressView.BeginCopy(Locale["MergeActions_ExportingToSdCard"])));
                }, ct);
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, string.Format(Locale["MergeActions_ErrorExportingProfile"], profile.Name, target.Label));
                await ErrorDialog.ShowAsync(ex);
            }
            finally {
                progressView.Hide();
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, string.Format(Locale["MergeActions_ErrorExportingProfile"], profile.Name, target.Label));
            await ErrorDialog.ShowAsync(ex);
        }
    }

    private static IEnumerable<DisplayDisk> EnumerateLocalExportDisks()
    {
        return DriveInfo.GetDrives()
            .Where(static driveInfo => {
                try {
                    if (driveInfo.DriveType is DriveType.Network or DriveType.CDRom
                        or DriveType.NoRootDirectory) {
                        return false;
                    }

                    return Directory.Exists(Path.Combine(driveInfo.RootDirectory.FullName, "atmosphere"));
                }
                catch {
                    return false;
                }
            })
            .Select(static driveInfo => new DisplayDisk(driveInfo));
    }

    private static async Task<ISdExportTarget?> CreateExportTarget(DisplayDisk selectedDisk)
    {
        if (selectedDisk.IsManualSelection) {
            var results = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
                Title = Locale["MergeActions_SelectSdCard"],
                AllowMultiple = false
            });

            if (results is not [{ } folder] || folder.TryGetLocalPath() is not { } localPath) {
                return null;
            }

            return SdExportTarget.FromFileSystem(localPath);
        }

        if (selectedDisk.IsMtp
            && OperatingSystem.IsWindows()
            && selectedDisk is { MtpDeviceId: { } deviceId, MtpRootPath: { } mtpRootPath }) {
            return SdExportTarget.FromMtp(deviceId, selectedDisk.MtpDeviceName ?? string.Empty, mtpRootPath);
        }

        return SdExportTarget.FromFileSystem(selectedDisk.RootPath);
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
#endif
}