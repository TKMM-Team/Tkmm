using Avalonia.Controls.Presenters;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Tkmm.Core;
using Tkmm.Core.Helpers;
using Tkmm.Dialogs;
using Tkmm.Wizard.Pages;

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
        Func<ValueTask> next = await EmulatorSelectionPage();

    Return:
        await next();
        
        bool result = await NextPage()
            .WithTitle(TkLocale.WizPageFinal_Title)
            .WithContent<GameLanguageSelectionPage>()
            .WithActionContent(TkLocale.WizPageFinal_Action_Finish)
            .Show();

        if (!result) {
            goto Return;
        }
    }

    private async ValueTask<Func<ValueTask>> EmulatorSelectionPage()
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
                return context.GetSelection() switch {
                    EmulatorSelection.Ryujinx => SetupRyujinxPage,
                    EmulatorSelection.Switch => EnsureConfigurationPage,
                    EmulatorSelection.Other => SetupEmulatorPage,
                    _ => throw new ArgumentException("Invalid emulator selection")
                };
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
        bool result = await NextPage()
            .WithTitle(TkLocale.SetupWizard_RyujinxSetup_Title)
            .WithContent(TkLocale.SetupWizard_RyujinxSetup_Content)
            .WithActionContent(TkLocale.SetupWizard_RyujinxSetup_Action)
            .Show();

        if (!result) {
            await EmulatorSelectionPage();
            return;
        }

        if (TkRyujinxHelper.UseRyujinx(out _).Case is string error) {
            object errorResult = await ErrorDialog.ShowAsync(new Exception(error),
                TaskDialogStandardResult.Retry, TaskDialogStandardResult.Cancel);
            
            if (errorResult is TaskDialogStandardResult.Retry) {
                goto Retry;
            }

            await EmulatorSelectionPage();
            return;
        }

        await EnsureConfigurationPage();
    }

    private async ValueTask SetupEmulatorPage()
    {
    Retry:
        string? emulatorFilePath = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = "Select emulator executable",
            AllowMultiple = false,
            FileTypeFilter = [
                _executableFilePattern
            ]
        }) switch {
            [IStorageFile target] => target.TryGetLocalPath(),
            _ => null
        };

        if (emulatorFilePath is null) {
            await EmulatorSelectionPage();
            return;
        }

        if (TkEmulatorHelper.UseEmulator(emulatorFilePath, out _).Case is string error) {
            object errorResult = await ErrorDialog.ShowAsync(new Exception(error),
                TaskDialogStandardResult.Retry, TaskDialogStandardResult.Cancel);

            if (errorResult is TaskDialogStandardResult.Retry) {
                goto Retry;
            }

            await EmulatorSelectionPage();
            return;
        }

        await EnsureConfigurationPage();
    }

    private async ValueTask EnsureConfigurationPage()
    {
    Verify:
        if (TKMM.TryGetTkRom() is not null) {
            return;
        }
        
        bool result = await NextPage()
            .WithTitle(TkLocale.SetupWizard_GameDumpConfigPage_Title)
            .WithContent(new GameDumpConfigPage {
                DataContext = new GameDumpConfigPageContext()
            })
            .WithActionContent(TkLocale.SetupWizard_GameDumpConfigPage_Action)
            .Show();

        if (result is false) {
            await EmulatorSelectionPage();
            return;
        }

        goto Verify;
    }
}