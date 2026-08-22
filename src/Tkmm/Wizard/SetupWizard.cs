using Avalonia.Controls.Presenters;
using Tkmm.Core;
using Tkmm.Wizard.Helpers;
using Tkmm.Wizard.Models;

namespace Tkmm.Wizard;

public sealed class SetupWizard(ContentPresenter presenter)
{
    private readonly Stack<string> _history = new();

    internal bool SkipApplicationLanguage { get; } = AppLanguageHelper.CheckSkipApplicationLanguageFlag();

#if !SWITCH
    internal DumpSource SelectedDumpSource { get; set; }
    internal string? EmulatorPathHint { get; set; }
#endif

    internal ContentPresenter Presenter => presenter;

    internal SetupWizardPageBuilder NextPage() => new(presenter);

    internal ValueTask<PageShowResult> FirstPage() => new SetupWizardPageBuilder(presenter, isFirstPage: true)
        .WithTitle(TkLocale.SetupWizard_FirstPage_Title)
        .WithContent(TkLocale.SetupWizard_FirstPage_Content)
        .WithActionContent(TkLocale.SetupWizard_FirstPage_Action)
        .Show();

    public async ValueTask Start()
    {
#if SWITCH
        TkConfig.Shared.SdCardRootPath = "/flash";
        TkConfig.Shared.KeysFolderPath = "/flash/switch";
#endif
        ClearHistory();
        var step = WizardSteps.Welcome;
        while (step is not WizardSteps.Done) {
            step = await RunStep(step);
        }

        TkConfig.Shared.Save();
        Config.Shared.Save();
    }

    private void ClearHistory() => _history.Clear();

    private async ValueTask<string> RunStep(string stepId)
    {
        if (_history.Count == 0 || _history.Peek() != stepId) {
            _history.Push(stepId);
        }

        var result = await WizardSteps.Run(stepId, this);
        if (!result.WentBack) {
            return result.NextStepId ?? WizardSteps.Done;
        }

        if (_history.Count > 0) {
            _history.Pop();
        }

        return _history.Count > 0 ? _history.Peek() : stepId;
    }
}
