using Avalonia.Controls.Presenters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Wizard.Views;

namespace Tkmm.Wizard;

public partial class SetupWizardPage(bool isFirstPage = false) : ObservableObject
{
    protected bool? _result;
    
    public bool IsFirstPage { get; } = isFirstPage;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private object? _content;

    [ObservableProperty]
    private object? _actionContent = Locale[TkLocale.Action_Next];

    [RelayCommand]
    private void MoveBack()
    {
        _result = false;
    }

    [RelayCommand]
    private void MoveNext()
    {
        _result = true;
    }
    
    public async ValueTask<bool> Show(ContentPresenter presenter)
    {
        presenter.Content = new SetupWizardPageView {
            DataContext = this,
        };

        return await Task.Run(() => {
            while (_result is null) {
            }

            return _result.Value;
        });
    }
}