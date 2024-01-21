using Avalonia.Controls;
using Avalonia.VisualTree;
using Tkmm.ViewModels.Pages;
using Avalonia.Markup.Xaml;
using Avalonia;

namespace Tkmm.Views.Pages;

public partial class ShopParamPageView : UserControl
{
    public ShopParamPageView()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
    {
        // Get a reference to the window
        var window = this.GetVisualRoot() as Window;
        if (window != null)
        {
            // Pass the window to the ViewModel
            DataContext = new ShopParamPageViewModel();
        }
    }
}