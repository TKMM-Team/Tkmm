#if !SWITCH

using Avalonia.Controls.Presenters;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Tkmm.Core;
using Tkmm.Core.Helpers;
using Tkmm.Dialogs;
using Tkmm.Wizard.Pages;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Wizard;

public sealed class StandardSetupWizard(ContentPresenter presenter) : SetupWizard(presenter)
{
    private static readonly FilePickerFileType _executableFilePattern = new("Executable") {
        Patterns = [
            OperatingSystem.IsWindows() ? "*.exe" : "*"
        ]
    };
    
    public override async ValueTask Start()
    {
        await FirstPage();
        
    Return:
        await EmulatorSelectionPage();
        
        bool result = await NextPage()
            .WithTitle(TkLocale.WizPageFinal_Title)
            .WithContent<GameLanguageSelectionPage>(new GameLanguageSelectionPageContext())
            .WithActionContent(TkLocale.WizPageFinal_Action_Finish)
            .Show();

        if (!result) {
            goto Return;
        }
    }

    private async ValueTask EmulatorSelectionPage()
    {
        EmulatorSelectionPageContext context = new();
        
    Retry:
        bool result = await NextPage()
            .WithTitle(TkLocale.SetupWizard_EmulatorSelection_Title)
            .WithContent<EmulatorSelectionPage>(context)
            .Show();

        switch (result) {
            case false:
                await FirstPage();
                goto Retry;
            case true when context.IsValid: {
                await (context.GetSelection() switch {
                    EmulatorSelection.Ryujinx => SetupRyujinxPage(),
                    EmulatorSelection.Other => SetupEmulatorPage(),
                    EmulatorSelection.Switch => ManualSetup(context),
                    EmulatorSelection.Manual => ManualSetup(context),
                    _ => throw new ArgumentException("Invalid selection")
                });
                return;
            }
        }

        await MessageDialog.Show(
            TkLocale.SetupWizard_Popup_InvalidEmulatorSelection_Content,
            TkLocale.SetupWizard_Popup_InvalidEmulatorSelection_Title);
        
        goto Retry;
    }

    private async ValueTask SetupRyujinxPage()
    {
    Retry:
        var result = await NextPage()
            .WithTitle(TkLocale.SetupWizard_RyujinxSetup_Title)
            .WithContent(TkLocale.SetupWizard_RyujinxSetup_Content)
            .WithActionContent(TkLocale.SetupWizard_RyujinxSetup_Action)
            .Show();

        if (!result) {
            await EmulatorSelectionPage();
            return;
        }

        if (TkRyujinxHelper.UseRyujinx(out _).Case is string error) {
            var errorResult = await ErrorDialog.ShowAsync(new Exception(error), forceShowInDebug: true,
                TaskDialogStandardResult.Retry, TaskDialogStandardResult.Cancel);
            
            if (errorResult is TaskDialogStandardResult.Retry) {
                goto Retry;
            }

            await ManualSetup(new EmulatorSelectionPageContext());
            return;
        }

        await EnsureConfigurationPage(warnInvalid: true);
    }

    private async ValueTask SetupEmulatorPage()
    {
        var emulatorFilePath = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = "Select emulator executable",
            AllowMultiple = false,
            FileTypeFilter = [
                _executableFilePattern
            ]
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

        if (emulatorFilePath is null) {
            await EmulatorSelectionPage();
            return;
        }

        try {
            if (TkEmulatorHelper.UseEmulator(emulatorFilePath, out _).Case is string error) {
                object errorResult = await ErrorDialog.ShowAsync(new Exception(error),
                    TaskDialogStandardResult.Retry, TaskDialogStandardResult.Cancel);

                if (errorResult is TaskDialogStandardResult.Retry) {
                    await ManualSetup(new EmulatorSelectionPageContext());
                }

                return;
            }
        }
        catch (Exception ex) {
            await ErrorDialog.ShowAsync(ex);
            await ManualSetup(new EmulatorSelectionPageContext());
            return;
        }

        await EnsureConfigurationPage(warnInvalid: true);
    }

    private async ValueTask ManualSetup(EmulatorSelectionPageContext context)
    {
        var romfsType = false;
        
        Start:
        if (context.GetSelection() != EmulatorSelection.Switch) {
            EmulatorNameInputPageContext nameContext = new();
            var nameResult = await NextPage()
                .WithTitle(TkLocale.SetupWizard_EmulatorNameInput_Title)
                .WithContent<EmulatorNameInputPage>(nameContext)
                .Show();

            if (!nameResult) {
                await EmulatorSelectionPage();
                return;
            }

            try {
                Config.Shared.EmulatorPath = nameContext.EmulatorName;
                if (Path.GetFileNameWithoutExtension(nameContext.EmulatorName).Equals("ryujinx", StringComparison.InvariantCultureIgnoreCase)) {
                    TkRyujinxHelper.UseRyujinx(out _, true);
                } else {
                    TkEmulatorHelper.UseEmulator(nameContext.EmulatorName, out _);
                }
            }
            catch {
                // If any error occurs, continue with manual setup
            }
        }

        if (TKMM.TryGetTkRom(out var initialHasBaseGameCheck, out _, out _) is not null) {
            goto MergeOutputSetup;
        }
        
    SelectDumpType:
        romfsType = false;
        if (!initialHasBaseGameCheck) {
            TkConfig.Shared.GameDumpFolderPaths?.Clear();
            
            BaseGameDumpTypePageContext baseGameContext = new();
            var dumpType = await NextPage()
                .WithTitle(TkLocale.SetupWizard_DumpType_Title)
                .WithContent<BaseGameDumpTypePage>(baseGameContext)
                .Show();

            if (!dumpType) {
                goto Start;
            }
            
            switch (baseGameContext.GetSelectedType()) {
                case BaseGameDumpType.XciNsp:
                    if (!await SetupXciNspBaseGame()) {
                        goto SelectDumpType;
                    }
                    break;
                case BaseGameDumpType.Romfs:
                    romfsType = true;
                    if (!await SetupRomfsBaseGame()) {
                        goto SelectDumpType;
                    }
                    break;
                case BaseGameDumpType.SdCard:
                    if (!await SetupSdCardBaseGame()) {
                        goto SelectDumpType;
                    }
                    break;
                case BaseGameDumpType.Nand:
                    if (!await SetupNandBaseGame()) {
                        goto SelectDumpType;
                    }
                    break;
                default:
                    if (!await SetupXciNspBaseGame()) {
                        goto SelectDumpType;
                    }
                    break;
            }
        }
        
        if (!romfsType) {
            if (TKMM.TryGetTkRom(out var hasBaseGame, out var hasUpdateInitialCheck, out _) is null && !hasBaseGame) {
                await MessageDialog.Show(
                    Locale[TkLocale.SetupWizard_BaseGameDumpConfigPage_InvalidConfiguration],
                    TkLocale.SetupWizard_BaseGameDumpConfigPage_InvalidConfiguration_Title);
                TkConfig.Shared.PackagedBaseGamePaths.Clear();
                goto SelectDumpType;
            }
            
    UpdateSelection:        
            if (hasUpdateInitialCheck) {
                goto MergeOutputSetup;
            }
            
            UpdateDumpTypePageContext updateContext = new();
            var updateDump = await NextPage()
                .WithTitle(TkLocale.SetupWizard_UpdateDumpType_Title)
                .WithContent<UpdateDumpTypePage>(updateContext)
                .Show();

            if (!updateDump) {
                goto SelectDumpType;
            }

            switch (updateContext.GetSelectedType()) {
                case UpdateDumpType.Nsp:
                    await SetupNspUpdate();
                    break;
                case UpdateDumpType.SdCard:
                    await SetupSdCardUpdate();
                    break;
                case UpdateDumpType.Nand:
                    await SetupNandUpdate();
                    break;
                default:
                    await SetupNspUpdate();
                    break;
            }
            
            if (TKMM.TryGetTkRom(out _, out bool hasUpdate, out _) is null && !hasUpdate) {
                await MessageDialog.Show(
                    Locale[TkLocale.SetupWizard_UpdateDumpConfigPage_InvalidConfiguration],
                    TkLocale.SetupWizard_UpdateDumpConfigPage_InvalidConfiguration_Title);
                TkConfig.Shared.PackagedUpdatePaths.Clear();
                goto UpdateSelection;
            }
        }
        else
        {
            if (TKMM.TryGetTkRom(out string? error) is not null) {
                goto MergeOutputSetup;
            }

            if (error is not null) {
                await MessageDialog.Show(error, TkLocale.TkExtensibleRomProvider_InvalidGameDump);
                romfsType = false;
                goto SelectDumpType;
            }
        }
        
    MergeOutputSetup:
        if (string.IsNullOrEmpty(Config.Shared.MergeOutput) && context.GetSelection() != EmulatorSelection.Switch) {
            MergeOutputSetupPageContext mergeContext = new();
            bool mergeOutput = await NextPage()
                .WithTitle(TkLocale.SetupWizard_MergeOutputSetup_Title)
                .WithContent<MergeOutputSetupPage>(mergeContext)
                .Show();

            if (!mergeOutput) {
                goto SelectDumpType;
            }

            if (Path.GetFileNameWithoutExtension(mergeContext.MergeOutputPath).Equals("0100f2c0115b6000", StringComparison.InvariantCultureIgnoreCase)) {
                mergeContext.MergeOutputPath = Path.Combine(mergeContext.MergeOutputPath, "TKMM");
            }
            
            Config.Shared.MergeOutput = mergeContext.MergeOutputPath;
        }
    }

    private async ValueTask<bool> SetupKeysIfNeeded()
    { 
        Retry:
        if (TkConfig.Shared.KeysFolderPath is not null &&
            TkKeyUtils.GetKeysFromFolder(TkConfig.Shared.KeysFolderPath) is not null) {
            return true;
        }
        KeysFolderPageContext keysContext = new();
        var result = await NextPage()
            .WithTitle(TkLocale.SetupWizard_KeysFolder_Title)
            .WithContent<KeysFolderPage>(keysContext)
            .Show();

        if (!result) {
            return false;
        }
            
        TkConfig.Shared.KeysFolderPath = keysContext.KeysFolderPath;
        TkConfig.Shared.Save();

        if (!Directory.Exists(keysContext.KeysFolderPath)) {
            goto MessageDialog;
        }

        if (TkKeyUtils.GetKeysFromFolder(keysContext.KeysFolderPath) is not null) {
            return true;
        } 
    MessageDialog:
        await MessageDialog.Show(
            TkLocale.SetupWizard_ManualSetup_MissingKeys_Content,
            TkLocale.SetupWizard_MissingKeys_Title);
        TkConfig.Shared.KeysFolderPath = null;
        TkConfig.Shared.Save();
        goto Retry;
    }

    private async ValueTask<bool> SetupXciNspBaseGame()
    {
        Retry:
        if (!await SetupKeysIfNeeded())
            return false;

        BaseGameSplitTypePageContext splitContext = new();
        var splitPage = await NextPage()
            .WithTitle(TkLocale.SetupWizard_BaseGameSplit_Title)
            .WithContent<BaseGameSplitTypePage>(splitContext)
            .Show();

        if (!splitPage) {
            return false;
        }

        if (splitContext.IsSplitFile) {
            var splitFolderPath = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
                Title = Locale[TkLocale.SetupWizard_SelectSplitFilesFolder],
                AllowMultiple = false
            }) switch {
                [var target] => target.TryGetLocalPath(),
                _ => null
            };

            if (splitFolderPath is null) {
                goto Retry;
            }

            TkConfig.Shared.PackagedBaseGamePaths.New(splitFolderPath);
        }
        else {
            var baseGamePath = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                Title = Locale[TkLocale.SetupWizard_SelectBaseGameFile],
                AllowMultiple = false,
                FileTypeFilter = [
                    new FilePickerFileType("XCI/NSP") {
                        Patterns = ["*.xci", "*.nsp"]
                    }
                ]
            }) switch {
                [var target] => target.TryGetLocalPath(),
                _ => null
            };

            if (baseGamePath is null) {
                goto Retry;
            }

            TkConfig.Shared.PackagedBaseGamePaths.New(baseGamePath);
        }
        return true;
    }

    private async ValueTask<bool> SetupRomfsBaseGame()
    {
        var romfsPath = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = Locale[TkLocale.SetupWizard_SelectRomfsFolder],
            AllowMultiple = false
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

        if (romfsPath is null) {
            return false;
        }

        TkConfig.Shared.GameDumpFolderPaths.New(romfsPath);
        return true;
    }

    private async ValueTask<bool> SetupSdCardBaseGame()
    {
        var sdCardPath = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = Locale[TkLocale.SetupWizard_SelectSdCardRoot],
            AllowMultiple = false
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

        if (sdCardPath is null) {
            return false;
        }

        TkConfig.Shared.SdCardRootPath = sdCardPath;
        TkConfig.Shared.KeysFolderPath = Path.Combine(sdCardPath, "switch");
        return true;
    }

    private async ValueTask<bool> SetupNandBaseGame()
    {
        if (!await SetupKeysIfNeeded())
            return false;

        var nandPath = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = Locale[TkLocale.SetupWizard_SelectNandFolder],
            AllowMultiple = false
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

        if (nandPath is null) {
            return false;
        }

        TkConfig.Shared.NandFolderPaths.New(nandPath);
        return true;
    }

    private async ValueTask SetupNspUpdate()
    {
        if (!await SetupKeysIfNeeded())
            return;

        var updatePath = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = Locale[TkLocale.SetupWizard_SelectUpdateNspFile],
            AllowMultiple = false,
            FileTypeFilter = [
                new FilePickerFileType("NSP") {
                    Patterns = ["*.nsp"]
                }
            ]
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

        if (updatePath is null) {
            return;
        }

        TkConfig.Shared.PackagedUpdatePaths.New(updatePath);
    }

    private async ValueTask SetupSdCardUpdate()
    {
        var sdCardPath = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = Locale[TkLocale.SetupWizard_SelectSdCardRoot],
            AllowMultiple = false
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

        if (sdCardPath is null) {
            return;
        }

        TkConfig.Shared.SdCardRootPath = sdCardPath;
        TkConfig.Shared.KeysFolderPath = Path.Combine(sdCardPath, "switch");
    }

    private async ValueTask SetupNandUpdate()
    {
        if (!await SetupKeysIfNeeded())
            return;

        var nandPath = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = Locale[TkLocale.SetupWizard_SelectNandFolder],
            AllowMultiple = false
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

        if (nandPath is null) {
            return;
        }

        TkConfig.Shared.NandFolderPaths.New(nandPath);
    }

    private async ValueTask EnsureConfigurationPage(bool warnInvalid = false)
    {
        if (TKMM.TryGetTkRom(out string? error) is not null) {
            return;
        }

        if (warnInvalid) {
            await MessageDialog.Show(
                error ?? Locale[TkLocale.SetupWizard_GameDumpConfigPage_InvalidConfiguration],
                TkLocale.SetupWizard_GameDumpConfigPage_InvalidConfiguration_Title);
        }
    }
}

#endif