using Avalonia;
using Avalonia.Controls.Presenters;
using Tkmm.Wizard.Helpers;
using Tkmm.Wizard.Models;

namespace Tkmm.Wizard;

public abstract class SetupWizard(ContentPresenter presenter)
{
    private readonly Stack<string> _history = new();

    protected bool SkipApplicationLanguage { get; } = AppLanguageHelper.CheckSkipApplicationLanguageFlag();

    public abstract ValueTask Start();

    internal ContentPresenter Presenter => presenter;

    internal SetupWizardPageBuilder NextPage() => new(presenter);

    internal void ClearHistory() => _history.Clear();

    internal ValueTask<bool> FirstPage() => new SetupWizardPageBuilder(presenter, isFirstPage: true)
        .WithTitle(TkLocale.SetupWizard_FirstPage_Title)
        .WithContent(TkLocale.SetupWizard_FirstPage_Content)
        .WithActionContent(TkLocale.SetupWizard_FirstPage_Action)
        .Show();

    protected async ValueTask<string> RunStep(string stepId, Func<ValueTask<StepResult>> show)
    {
        if (_history.Count == 0 || _history.Peek() != stepId) {
            _history.Push(stepId);
        }

        var result = await show();
        if (!result.WentBack) {
            return result.NextStepId ?? WizardSteps.Done;
        }

        if (_history.Count > 0) {
            _history.Pop();
        }

        return _history.Count > 0 ? _history.Peek() : stepId;
    }

    internal async ValueTask<(bool Next, WizardRadioOption? Selected)> ChooseAsync(
        TkLocale title,
        IReadOnlyList<WizardRadioOption> options,
        string groupName,
        string? description = null,
        string? mutedFooter = null,
        params string[] extraDescriptions)
    {
        var page = NextPage().WithTitle(title).BeginContent();
        if (description is not null) {
            page.AddText(description, new Thickness(0, 0, 0, 10));
        }

        foreach (var extra in extraDescriptions) {
            page.AddText(extra, new Thickness(0, 0, 0, 8));
        }

        page.AddRadioGroup(groupName, options);
        if (mutedFooter is not null) {
            page.WithMutedFooter(mutedFooter);
        }

        if (!await page.Show()) {
            return (false, null);
        }

        return (true, options.FirstOrDefault(o => o is { IsSelected: true, IsVisible: true, IsEnabled: true }));
    }

    internal async ValueTask<(bool Next, string? Path)> ShowFolderPathAsync(
        TkLocale title,
        string description,
        string browseTitle,
        string? header = null,
        string? initialPath = null)
    {
        WizardPathField field = new() {
            Header = header,
            Text = initialPath ?? string.Empty,
            Browse = new WizardBrowseOptions {
                Title = browseTitle
            }
        };

        var page = NextPage().WithTitle(title).BeginContent()
            .AddText(description, new Thickness(0, 0, 0, 10))
            .AddPathField(field);

        return await page.Show() ? (true, field.Text) : (false, null);
    }
}