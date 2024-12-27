using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.ViewModels.Wizard;

namespace Tkmm.Components.Wizard;

public partial class WizAction(object? content, int selection, Func<Task<bool>>? onMoveNext = null) : ObservableObject
{
    private readonly int _selection = selection;
    
    [ObservableProperty]
    private object? _content = content;

    public Func<Task<bool>>? OnMoveNext { get; } = onMoveNext;

    [RelayCommand]
    private async Task MoveNext(WizPageViewModel currentPage)
    {
        if (OnMoveNext is null || await OnMoveNext.Invoke()) {
            WizLayout.NextPage(currentPage, _selection);
        }
    }
}