#if SWITCH
using Avalonia.Controls.Presenters;
using Tkmm.Core;
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
            if (TKMM.TryGetTkRom() is not null)
            {
                goto LangPage;
            }

            bool result = await NextPage()
                .WithTitle(TkLocale.SetupWizard_GameDumpConfigPage_Title)
                .WithContent<NxDumpConfigPage>(new GameDumpConfigPageContext())
                .WithActionContent(TkLocale.SetupWizard_GameDumpConfigPage_Action)
                .WithHelpContent(TkLocale.SetupWizard_NxDumpConfigPage_Help)
                .Show();

            if (!result)
            {
                goto FirstPage;
            }
            
            if (TKMM.TryGetTkRom() is null)
            {
                goto Verify;
            }

        LangPage:
            bool langResult = await NextPage()
                .WithTitle(TkLocale.WizPageFinal_Title)
                .WithContent<GameLanguageSelectionPage>()
                .WithActionContent(TkLocale.WizPageFinal_Action_Finish)
                .Show();

            if (!langResult)
            {
                if (TKMM.TryGetTkRom() is not null)
                {
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