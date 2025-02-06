using Avalonia.Controls.Presenters;
using Tkmm.Core;
using Tkmm.Dialogs;
using Tkmm.Wizard.Pages;

namespace Tkmm.Wizard
{
    public sealed class NxSetupWizard(ContentPresenter presenter) : SetupWizard(presenter)
    {
        public override async ValueTask Start()
        {
            // Set configuration paths early so that the verification check can succeed
            TkConfig.Shared.SdCardRootPath = "F:\\TOTK\\Test Stuff\\SdCard";
            TkConfig.Shared.KeysFolderPath = "F:\\TOTK\\Test Stuff\\SdCard\\switch";

            // Show the same first page as in the standard wizard.
            await FirstPage();

        Verify:
            // If configuration is already valid, skip the dump config page.
            if (TKMM.TryGetTkRom() is not null)
            {
                goto LangPage;
            }

            // Show the NX dump configuration page.
            bool dumpResult = await NextPage()
                .WithTitle(TkLocale.SetupWizard_GameDumpConfigPage_Title)
                // Use the NX dump page (with keys and SD paths set statically)
                .WithContent<NxDumpConfigPage>(new NxDumpConfigPageContext())
                // Use the "Verify" button text (same as in GameDumpConfigPage) so that the button is not empty.
                .WithActionContent(TkLocale.SetupWizard_GameDumpConfigPage_Action)
                .Show();

            if (!dumpResult)
            {
                // If the user clicks Back, return to the first page.
                await FirstPage();
                goto Verify;
            }

            // After the NX dump config page finishes, check the configuration again.
            if (TKMM.TryGetTkRom() is null)
            {
                // If still invalid, re-show the dump config page.
                goto Verify;
            }

        LangPage:
            // Show the final game language selection page.
            bool langResult = await NextPage()
                .WithTitle(TkLocale.WizPageFinal_Title)
                .WithContent<GameLanguageSelectionPage>()
                .WithActionContent(TkLocale.WizPageFinal_Action_Finish)
                .Show();

            if (!langResult)
            {
                goto LangPage;
            }

            // Save the configuration if everything is complete.
            TkConfig.Shared.Save();
            Config.Shared.Save();
        }
    }
} 