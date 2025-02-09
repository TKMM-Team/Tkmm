using Avalonia.Controls.Presenters;

namespace Tkmm.Wizard;

public abstract class SetupWizard(ContentPresenter presenter)
{
    private readonly ContentPresenter _presenter = presenter;
    
    /// <summary>
    /// Define the logic and flow of this <see cref="SetupWizard"/>.
    /// </summary>
    public abstract ValueTask Start();

    protected ValueTask<bool> FirstPage()
    {
        SetupWizardPageBuilder builder = new(_presenter, isFirstPage: true);
        
        return builder
            .WithTitle(TkLocale.SetupWizard_FirstPage_Title)
            .WithContent(TkLocale.SetupWizard_FirstPage_Content)
            .WithActionContent(TkLocale.SetupWizard_FirstPage_Action)
            .Show();
    }

    protected SetupWizardPageBuilder NextPage()
    {
        return new SetupWizardPageBuilder(_presenter);
    }
}