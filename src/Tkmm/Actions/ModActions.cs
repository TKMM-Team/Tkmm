using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
using Tkmm.Core.Abstractions;
using Tkmm.Dialogs;

namespace Tkmm.Actions;

public sealed partial class ModActions : GuardedActionGroup<ModActions>
{
    protected override string ActionGroupName { get; } = nameof(ModActions).Humanize();
    
    [RelayCommand]
    public async Task ExportMod()
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile.Selected is not ITkProfileMod target) {
            return;
        }

        IStorageFile? result = await App.XamlRoot.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = "Export Mod",
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
            // TODO: Export packaged mod
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured while exporting the mod '{ModName}' to '{TargetFile}'.",
                target.Mod.Name, result.Name);
            await ErrorDialog.ShowAsync(ex);
        }

        // TODO: Fetch message template from locale
        TkStatus.SetTemporaryShort($"'{target.Mod.Name}' was exported successfully!", TkIcons.CIRCLE_CHECK);
    }
    
    [RelayCommand]
    public async Task ConfigureModOptions()
    {
        await CanActionRun(showError: false);
        
        if (TKMM.ModManager.CurrentProfile.Selected is ITkProfileMod target) {
            target.IsEditingOptions = !target.IsEditingOptions;
        }
    }

    public async Task RemoveModFromProfile()
    {
        await CanActionRun(showError: false);
        
        if (TKMM.ModManager.CurrentProfile.Selected is ITkProfileMod target) {
            TKMM.ModManager.CurrentProfile.Mods.Remove(target);
        }
    }

    [RelayCommand]
    public async Task UninstallMod()
    {    
        if (TKMM.ModManager.CurrentProfile.Selected is not ITkProfileMod target) {
            await CanActionRun(showError: false);
            return;
        }

        await UninstallMod(target.Mod);
    }
    
    public async Task UninstallMod(ITkMod target)
    {
        await CanActionRun(showError: false);

        ContentDialog dialog = new() {
            Title = "Permenently uninstall?",
            Content = $"""
                WARNING: THIS CANNOT BE UNDONE
                
                Are you sure you would like to permenently uninstall the mod '{target.Name}'?
                """,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = "Uninstall",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return;
        }
        
        try {
            // TODO: Uninstall the target mod
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured while uninstalling the mod '{ModName}'. " +
                                     "Manual cleanup may be required.", target.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }
}