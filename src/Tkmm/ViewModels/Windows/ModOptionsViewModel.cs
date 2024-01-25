using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.ViewModels.Pages;

namespace Tkmm.ViewModels.Dialogs;

public partial class ModOptionsViewModel(PackagingPageViewModel toolsPageViewModel) : ObservableObject
{
    private readonly PackagingPageViewModel _toolsPageViewModel = toolsPageViewModel;

    [RelayCommand]
    private Task Add()
    {
        throw new NotImplementedException();
    }
}
