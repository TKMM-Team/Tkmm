using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Abstractions;
using Tkmm.Core;
using Tkmm.Dialogs;

namespace Tkmm.Actions;

public sealed partial class ModActions : GuardedActionGroup<ModActions>
{
    protected override string ActionGroupName { get; } = nameof(ModActions).Humanize();

    public Task<ITkMod?> Install<T>(T? input, Stream? stream = null, ModContext context = default, CancellationToken ct = default) where T : class
        => Install(input, TKMM.ModManager.CurrentProfile, stream, context, ct);

    public async Task<ITkMod?> Install<T>(T? input, ITkProfile profile, Stream? stream = null, ModContext context = default, CancellationToken ct = default) where T : class
    {
        await CanActionRun(showError: false);

        try {
            if (await TKMM.ModManager.Install(input, stream, context, ct) is not ITkMod mod) {
                // TODO: Fetch message template from locale
                TKMM.Logger.LogError("The input of type '{InputType}' ('{Input}') failed to install.",
                    typeof(T), input);
                return null;
            }

            ITkProfileMod profileMod = mod.GetProfileMod();
            profile.Mods.Add(profileMod);
            profile.Selected = profileMod;
            
            // TODO: Fetch message template from locale
            TkStatus.SetTemporaryShort($"{mod.Name} was installed successfully!", TkIcons.CIRCLE_CHECK);
            return mod;
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured while installing an input of type '{InputType}' ('{Input}').",
                typeof(T), input);
            await ErrorDialog.ShowAsync(ex);
        }

        return null;
    }

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
    public async Task OpenModFolder()
    {
        await CanActionRun(showError: false);
        
        if (TKMM.ModManager.CurrentProfile.Selected is not ITkProfileMod target) {
            return;
        }

        string outputModFolder = Path.Combine(ModManager.SystemModsFolder, target.Mod.Id.ToString());

        try {
            ProcessStartInfo info = new() {
                FileName = outputModFolder,
                UseShellExecute = true
            };

            Process.Start(info);
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured while opening the mod folder for '{ModName}'.",
                target.Mod.Name);
            await ErrorDialog.ShowAsync(ex);
        }
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

        if (TKMM.ModManager.CurrentProfile.Selected is not ITkProfileMod target) {
            return;
        }

        int removeIndex = TKMM.ModManager.CurrentProfile.Mods.IndexOf(target);
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