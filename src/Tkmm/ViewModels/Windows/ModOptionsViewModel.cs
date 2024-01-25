using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.ViewModels.Pages;

namespace Tkmm.ViewModels.Dialogs;

public partial class ModOptionsViewModel(ToolsPageViewModel toolsPageViewModel) : ObservableObject
{
    private readonly ToolsPageViewModel _toolsPageViewModel = toolsPageViewModel;

    [RelayCommand]
    private Task Add()
    {
        throw new NotImplementedException();
    }
}
