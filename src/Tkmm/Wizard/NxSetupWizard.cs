#if SWITCH
using Avalonia.Controls.Presenters;
using Tkmm.Core;
using Tkmm.Wizard.WizardPages;

namespace Tkmm.Wizard;

public sealed class NxSetupWizard(ContentPresenter presenter) : SetupWizard(presenter)
{
    public override async ValueTask Start()
    {
        TkConfig.Shared.SdCardRootPath = "/flash";
        TkConfig.Shared.KeysFolderPath = "/flash/switch";

        ClearHistory();
        var step = WizardSteps.Welcome;

        while (step is not WizardSteps.Done) {
            step = await RunStep(step, step switch {
                WizardSteps.Welcome => async () => {
                    await FirstPage();
                    return StepResult.Next(SkipApplicationLanguage ? WizardSteps.Wifi : WizardSteps.ApplicationLanguage);
                },
                WizardSteps.ApplicationLanguage => () => SharedWizardPages.ApplicationLanguage(this, WizardSteps.Wifi),
                WizardSteps.Wifi => () => NxWizardPages.Wifi(this),
                WizardSteps.MissingKeys => () => NxWizardPages.MissingKeys(this),
                WizardSteps.VerifyDump => () => NxWizardPages.VerifyDump(this),
                WizardSteps.Firmware => () => SharedWizardPages.Firmware(this),
                WizardSteps.GameLanguage => () => SharedWizardPages.GameLanguage(this),
                _ => static () => ValueTask.FromResult(StepResult.Done())
            });
        }

        TkConfig.Shared.Save();
        Config.Shared.Save();
    }
} 
#endif