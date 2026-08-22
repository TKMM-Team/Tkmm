using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Tkmm.Core;
using Tkmm.Core.Models;
using Tkmm.Dialogs;
using Tkmm.Wizard.Helpers;
using Tkmm.Wizard.Models;
using Tkmm.Wizard.Pages;

namespace Tkmm.Wizard.WizardPages;

internal static class SharedWizardPages
{
    public static async ValueTask<StepResult> ApplicationLanguage(SetupWizard wizard, string nextStep)
    {
        var languages = Config.Shared.GetLanguagesInternal();
        var selected = languages.FirstOrDefault(language => language.Value == Config.Shared.CultureName.Value);
        if (string.IsNullOrEmpty(selected.Value)) {
            selected = languages[0];
        }

        ComboBox languageBox = new() {
            ItemsSource = languages,
            SelectedItem = selected,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinHeight = 36,
            DisplayMemberBinding = new Binding(nameof(SystemLanguage.DisplayName))
        };

        languageBox.SelectionChanged += (_, _) => {
            if (languageBox.SelectedItem is SystemLanguage language) {
                Config.Shared.CultureName = language;
            }
        };

        Config.Shared.CultureName = selected;

        if (!await wizard.NextPage()
                .WithTitle(TkLocale.Config_SystemLanguage)
                .BeginContent()
                .AddText(Locale[TkLocale.SetupWizard_ApplicationLanguage_Description], new Thickness(0, 0, 0, 10))
                .AddControl(languageBox)
                .Show()) {
            return StepResult.Back();
        }

        if (await MessageDialog.Show(
                TkLocale.SetupWizard_ApplicationLanguage_RestartPrompt,
                TkLocale.Action_Restart,
                MessageDialogButtons.YesNo) is MessageDialogResult.Yes) {
            AppLanguageHelper.RequestRestartToApplyLanguage();
        }

        return StepResult.Next(nextStep);
    }

    public static async ValueTask<StepResult> Firmware(SetupWizard wizard, bool showEmulatorNote = false)
    {
        string[] notes = showEmulatorNote
            ? [Locale[TkLocale.SetupWizard_Firmware_EmulatorNote]]
            : [];

        var (next, selected) = await wizard.ChooseAsync(
            TkLocale.SetupWizard_Firmware_Title,
            [
                WizardRadioOption.Opt(Locale[TkLocale.Config_Firmware_19OrLower], "Firmware19OrLower", selected: true),
                WizardRadioOption.Opt(Locale[TkLocale.Config_Firmware_20OrHigher], "Firmware20OrHigher")
            ],
            "firmware",
            Locale[TkLocale.SetupWizard_Firmware_Description],
            Locale[TkLocale.Config_SwitchFirmwareVersionDescription],
            notes);

        if (!next) {
            return StepResult.Back();
        }

        if (selected?.Tag is not string version) {
            return StepResult.Next(WizardSteps.Firmware);
        }

        Config.Shared.SwitchFirmwareVersion = new SwitchFirmwareVersion(version);
        return StepResult.Next(WizardSteps.GameLanguage);
    }

    public static async ValueTask<StepResult> GameLanguage(SetupWizard wizard)
        => await wizard.NextPage()
            .WithTitle(TkLocale.WizPageFinal_Title)
            .WithContent<GameLanguageSelectionPage>(new GameLanguageSelectionPageContext())
            .WithActionContent(TkLocale.WizPageFinal_Action_Finish)
            .Show()
            ? StepResult.Done()
            : StepResult.Back();
}