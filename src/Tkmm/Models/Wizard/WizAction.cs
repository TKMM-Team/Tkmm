using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.ViewModels.Wizard;

namespace Tkmm.Models.Wizard;

public partial class WizAction(object? content, int selection, string? classes = null) : ObservableObject
{
    private readonly int _selection = selection;
    
    [ObservableProperty]
    private object? _content = content;
    
    [ObservableProperty]
    private string _classes = classes ?? "";

    [RelayCommand]
    private void MoveNext(WizPageViewModel currentPage)
    {
        WizLayout.NextPage(currentPage, _selection);
    }
}