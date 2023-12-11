using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Tcml.Views.Pages;

namespace Tcml.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string _status = "Ready";

    [ObservableProperty]
    private UserControl _page = new HomePageView();
}
