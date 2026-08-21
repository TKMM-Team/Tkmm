#if !SWITCH
using Avalonia.Controls.Presenters;
using Tkmm.Core;
using Tkmm.Wizard.WizardPages;

namespace Tkmm.Wizard;

public sealed class DesktopSetupWizard(ContentPresenter presenter) : SetupWizard(presenter)
{
    internal bool ShowSwitchDumpOption { get; set; } = true;
    internal DumpSource DumpSource { get; set; } = DumpSource.Ryujinx;
    internal string? EmulatorPathHint { get; set; }

    public override async ValueTask Start()
    {
        ClearHistory();
        var step = WizardSteps.Welcome;

        while (step is not WizardSteps.Done) {
            step = await RunStep(step, step switch {
                WizardSteps.Welcome => async () => {
                    await FirstPage();
                    return StepResult.Next(SkipApplicationLanguage ? WizardSteps.Mode : WizardSteps.ApplicationLanguage);
                },
                WizardSteps.ApplicationLanguage => () => SharedWizardPages.ApplicationLanguage(this, WizardSteps.Mode),
                WizardSteps.Mode => () => DesktopWizardPages.Mode(this),
                WizardSteps.NxRecommend => () => DesktopWizardPages.NxRecommend(this),
                WizardSteps.DumpSource => () => DesktopWizardPages.DumpSourceStep(this),
                WizardSteps.Ryujinx => () => DesktopWizardPages.Ryujinx(this),
                WizardSteps.Manual => () => ManualDumpPages.Show(this),
                WizardSteps.Firmware => () => SharedWizardPages.Firmware(this, showEmulatorNote: !Config.Shared.TkmmMode.IsSwitch),
                WizardSteps.GameLanguage => () => SharedWizardPages.GameLanguage(this),
                _ => static () => ValueTask.FromResult(StepResult.Done())
            });
        }

        TkConfig.Shared.Save();
        Config.Shared.Save();
    }
}

public enum DumpSource { Ryujinx = 1, Switch = 2, Other = 3 }
public enum BaseGameDumpType { XciNsp, Romfs, SdCard, Nand }
public enum UpdateDumpType { Nsp, SdCard, Nand }
#endif