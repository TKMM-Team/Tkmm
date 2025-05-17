#if SWITCH
using Avalonia.Controls.Presenters;
using Tkmm.Core;
using Tkmm.Dialogs;
using Tkmm.Models.MenuModels;
using Tkmm.Wizard.Pages;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Wizard
{
    public sealed class NxSetupWizard(ContentPresenter presenter) : SetupWizard(presenter)
    {
        public override async ValueTask Start()
        {
            TkConfig.Shared.SdCardRootPath = "/flash";
            TkConfig.Shared.KeysFolderPath = "/flash/switch";

        FirstPage:
            await FirstPage();
            
            if (!TkKeyUtils.TryGetKeys(TkConfig.Shared.SdCardRootPath, out var keys))
            {
                bool proceed = await NextPage()
                    .WithTitle(TkLocale.SetupWizard_MissingKeys_Title)
                    .WithContent(TkLocale.SetupWizard_MissingKeys_Content)
                    .WithActionContent(TkLocale.Menu_NxReboot)
                    .Show();
                
                if (!proceed) {
                    goto FirstPage;
                }
                NxMenuModel.Reboot();
                await Task.Delay(-1);
            }
  
        Verify:
            if (TKMM.TryGetTkRom(out string? error) is not null) {
                goto LangPage;
            }

            if (error is not null) {
                await MessageDialog.Show(error, TkLocale.SetupWizard_GameDumpConfigPage_InvalidConfiguration_Title);
            }

            bool oopsie = await NextPage()
                .WithTitle(TkLocale.SetupWizard_MissingDump_Title)
                .WithContent(TkLocale.SetupWizard_MissingDump_Content)
                .WithActionContent(TkLocale.Menu_NxReboot)
                .Show();

            if (!oopsie) {
                goto FirstPage;
            }

            NxMenuModel.Reboot();
            await Task.Delay(-1);
            
        LangPage:
            bool langResult = await NextPage()
                .WithTitle(TkLocale.WizPageFinal_Title)
                .WithContent<GameLanguageSelectionPage>()
                .WithActionContent(TkLocale.WizPageFinal_Action_Finish)
                .Show();

            if (!langResult)
            {
                if (TKMM.TryGetTkRom() is not null) {
                    goto FirstPage;
                }
                goto Verify;
            }

            TkConfig.Shared.Save();
            Config.Shared.Save();
        }
    }
} 
#endif