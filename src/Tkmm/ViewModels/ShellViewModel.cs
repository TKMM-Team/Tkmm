using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Views.Pages;

namespace Tkmm.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string _status = "Ready";

    [ObservableProperty]
    private UserControl _page = new HomePageView();
}
