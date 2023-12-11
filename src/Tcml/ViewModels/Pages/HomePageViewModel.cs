using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tcml.ViewModels.Pages;

public partial class HomePageViewModel : ObservableObject
{
    [RelayCommand]
    public void SetStatus()
    {
        AppStatus.Set("Loading", "fa-solid fa-diagram-project", true);
    }
}
