using Avalonia.Controls;
using Tkmm.Core.Components;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;
public partial class AboutPageView : UserControl
{
    public AboutPageView()
    {
        InitializeComponent();
        DataContext = new AboutPageViewModel();
        FetchSync();
    }

    public static async void FetchSync()
    {
        await WikiSourceManager.Shared.Fetch();
    }
}
