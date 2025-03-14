using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;

namespace Tkmm.Wizard;

public class SetupWizardPageBuilder(ContentPresenter presenter, bool isFirstPage = false)
{
    private readonly ContentPresenter _presenter = presenter;
    private readonly SetupWizardPage _page = new(isFirstPage);

    public SetupWizardPage Build()
    {
        return _page;
    }

    public SetupWizardPageBuilder WithTitle(TkLocale title) => WithTitle(Locale[title]);

    public SetupWizardPageBuilder WithTitle(string title)
    {
        _page.Title = title;
        return this;
    }

    public SetupWizardPageBuilder WithContent(TkLocale content) => WithContent(Locale[content]);

    public SetupWizardPageBuilder WithContent(object? content)
    {
        if (content is StyledElement { Parent: ContentControl parent }) {
            parent.Content = null;
        }
        
        _page.Content = content;
        return this;
    }

    public SetupWizardPageBuilder WithHelpContent(TkLocale content) => WithHelpContent(Locale[content]);

    public SetupWizardPageBuilder WithHelpContent(object? content)
    {
        _page.HelpContent = content;
        return this;
    }

    public SetupWizardPageBuilder WithContent<TControl>(object? context = null) where TControl : Control, new()
    {
        _page.Content = new TControl {
            DataContext = context
        };
        
        return this;
    }

    public SetupWizardPageBuilder WithActionContent(TkLocale content) => WithActionContent(Locale[content]);

    public SetupWizardPageBuilder WithActionContent(object? content)
    {
        _page.ActionContent = content;
        return this;
    }

    public ValueTask<bool> Show()
    {
        return _page.Show(_presenter);
    }
}