using Avalonia.Controls.Presenters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Wizard.Views;

namespace Tkmm.Wizard;

public partial class SetupWizardPage(bool isFirstPage = false) : ObservableObject
{
    private bool? _result;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public bool IsFirstPage { get; } = isFirstPage;

    [ObservableProperty]
    public partial string? Title { get; set; }
    [ObservableProperty]
    public partial object? Content { get; set; }
    [ObservableProperty]
    public partial object? Action { get; set; } = Locale[TkLocale.Action_Next];

    [RelayCommand]
    private void MoveBack()
    {
        _result = false;
        _cancellationTokenSource.Cancel();
    }

    [RelayCommand]
    private void MoveNext()
    {
        _result = true;
        _cancellationTokenSource.Cancel();
    }
    
    public async ValueTask<bool> Show(ContentPresenter presenter)
    {
        presenter.Content = new SetupWizardPageView {
            DataContext = this,
        };

        try {
            await Task.Delay(-1, _cancellationTokenSource.Token);
        }
        catch (TaskCanceledException) {
            return _result ?? false;
        }

        return false;
    }
}