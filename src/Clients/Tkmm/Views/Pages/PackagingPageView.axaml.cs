using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class PackagingPageView : UserControl
{
    public PackagingPageView()
    {
        InitializeComponent();
        DataContext = new PackagingPageViewModel();
    }
}