using System.Diagnostics;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Components;
using Tkmm.Core;
using Tkmm.Dialogs;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Actions;

public sealed partial class ModActions : GuardedActionGroup<ModActions>
{
    protected override string ActionGroupName { get; } = nameof(ModActions).Humanize();

    public Task<TkMod?> Install(object input, Stream? stream = null, TkModContext? context = null, CancellationToken ct = default)
        => Install(input, TKMM.ModManager.GetCurrentProfile(), stream, context, ct);

    private async Task<TkMod?> Install(object input, TkProfile profile, Stream? stream = null, TkModContext? context = null, CancellationToken ct = default)
    {
        await CanActionRun(showError: false);

        try {
            TkStatus.Set(Locale[TkLocale.Status_Importing, input], TkIcons.GEAR_FOLDER, StatusType.Working);
            if (await TKMM.Install(input, stream, context, profile, ct) is not { } mod) {
                TkLog.Instance.LogError("The input of type '{InputType}' ('{Input}') failed to install.",
                    input.GetType(), input);
                return null;
            }

            TkStatus.SetTemporary(Locale[TkLocale.Status_ModSuccessfullyInstalled, mod.Name], TkIcons.CIRCLE_CHECK);
            return mod;
        }
        catch (Exception ex) {
            TkStatus.SetTemporary(Locale[TkLocale.Status_ModFailedInstalling, input], TkIcons.ERROR);
            TkLog.Instance.LogError(ex, "An error occured while installing an input of type '{InputType}' ('{Input}').",
                input.GetType(), input);
            await ErrorDialog.ShowAsync(ex);
        }

        return null;
    }

    [RelayCommand]
    public async Task ExportMod()
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile?.Selected is not { } target) {
            return;
        }

        var result = await App.XamlRoot.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = Locale[TkLocale.Menu_ModExportFile],
            DefaultExtension = ".tkcl",
            FileTypeChoices = [
                ImportActions.TkclFormat
            ],
            ShowOverwritePrompt = true,
            SuggestedFileName = target.Mod.Name
        });

        if (result is null) {
            return;
        }

        try {
            TkStatus.Set(Locale[TkLocale.Status_ExportingMod, target.Mod.Name, result.Name], TkIcons.LIST_CHECK);
            
            await using (var output = await result.OpenWriteAsync()) {
                await TKMM.ExportPackage(target.Mod, output);
            }
            TkStatus.SetTemporaryShort(Locale[TkLocale.Status_ModSuccessfullyExported, target.Mod.Name], TkIcons.CIRCLE_CHECK);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while exporting the mod '{ModName}' to '{TargetFile}'.",
                target.Mod.Name, result.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public async Task ExportModRomfs(CancellationToken ct = default)
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile?.Selected is not { } target) {
            return;
        }

        var targetOutputFolder = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = Locale[TkLocale.Menu_ModExportFolder],
            AllowMultiple = false,
            SuggestedFileName = target.Mod.Name
        }) switch {
            [var first, ..] when first.TryGetLocalPath() is { } path => path,
            _ => null
        };

        if (targetOutputFolder is null) {
            return;
        }

        try {
            TkStatus.Set(Locale[TkLocale.Status_ExportingMod, target.Mod.Name, targetOutputFolder], TkIcons.GEAR_FOLDER);
            
            await TKMM.ExportRomfs(target.Mod, targetOutputFolder, ct);
            TkStatus.SetTemporaryShort(Locale[TkLocale.Status_ModSuccessfullyExported, target.Mod.Name], TkIcons.CIRCLE_CHECK);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while exporting the mod '{ModName}' to '{TargetFolder}'.",
                target.Mod.Name, targetOutputFolder);
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public async Task ExportModAsProject(CancellationToken ct = default)
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile?.Selected is not { } target) {
            return;
        }

        var targetOutputFolder = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = Locale[TkLocale.Menu_ModExportProject],
            AllowMultiple = false,
            SuggestedFileName = target.Mod.Name
        }) switch {
            [var first, ..] when first.TryGetLocalPath() is { } path => path,
            _ => null
        };

        if (targetOutputFolder is null) {
            return;
        }

        try {
            TkStatus.Set(Locale[TkLocale.Status_ExportingMod, target.Mod.Name, targetOutputFolder], TkIcons.GEAR_FOLDER);

            using var tkRom = TKMM.GetTkRom();
            await TkProjectExporter.ExportAsync(target.Mod, TKMM.ModManager, tkRom, targetOutputFolder, ct);

            TkStatus.SetTemporaryShort(Locale[TkLocale.Status_ModSuccessfullyExported, target.Mod.Name], TkIcons.CIRCLE_CHECK);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while exporting the Mod '{ModName}' as a Project to '{TargetFolder}'.",
                target.Mod.Name, targetOutputFolder);
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public async Task OpenModFolder()
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile?.Selected is not { } target) {
            return;
        }

        var outputModFolder = Path.Combine(TKMM.ModManager.ModsFolderPath, target.Mod.Id.ToString());

        try {
            ProcessStartInfo info = new() {
                FileName = outputModFolder,
                UseShellExecute = true
            };

            Process.Start(info);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while opening the mod folder for '{ModName}'.",
                target.Mod.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public async Task ConfigureModOptions()
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile?.Selected is not { } target) {
            return;
            
        }
        
        if (target.Mod.OptionGroups.Sum(x => x.Options.Count) == 0) {
            await MessageDialog.Show(TkLocale.NoModOptions_Content, TkLocale.NoModOptions_Title);
            return;
        }
        
        target.IsEditingOptions = !target.IsEditingOptions;
    }

    public async Task RemoveModFromProfile()
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile?.Selected is not { } target) {
            return;
        }

        var removeIndex = TKMM.ModManager.CurrentProfile.Mods.IndexOf(target);
        TKMM.ModManager.CurrentProfile.Mods.RemoveAt(removeIndex);

        if (TKMM.ModManager.CurrentProfile.Mods.Count is 0) {
            return;
        }

        while (removeIndex >= TKMM.ModManager.CurrentProfile.Mods.Count) {
            removeIndex--;
        }

        TKMM.ModManager.CurrentProfile.Selected = TKMM.ModManager.CurrentProfile.Mods[removeIndex];
    }

    [RelayCommand]
    public async Task UninstallMod()
    {
        if (TKMM.ModManager.CurrentProfile?.Selected is not { } target) {
            await CanActionRun(showError: false);
            return;
        }

        await UninstallMod(target.Mod);
    }

    public async Task UninstallMod(TkMod target)
    {
        await CanActionRun(showError: false);

        ContentDialog dialog = new() {
            Title = Locale[TkLocale.System_Popup_UninstallMod_Title],
            Content = Locale[TkLocale.System_Popup_UninstallMod_Content, target.Name],
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = Locale[TkLocale.Action_Uninstall],
            SecondaryButtonText = Locale[TkLocale.Action_Cancel],
            DefaultButton = ContentDialogButton.Secondary,
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return;
        }

        try {
            TkImageResolver.CleanTarget(target.Id.ToString());
            TKMM.ModManager.Uninstall(target);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while uninstalling the mod '{ModName}'. " +
                                        "Manual cleanup may be required.", target.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }
}