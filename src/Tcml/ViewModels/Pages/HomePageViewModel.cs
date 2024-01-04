using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tcml.ViewModels.Pages;

public partial class HomePageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _description = string.Empty;

    [RelayCommand]
    public void SetStatus()
    {
        AppStatus.Set("Loading", "fa-solid fa-diagram-project", true);
    }
}
