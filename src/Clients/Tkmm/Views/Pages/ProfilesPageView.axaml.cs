using Avalonia;
using Avalonia.Controls;

namespace Tkmm.Views.Pages;

public partial class ProfilesPageView : UserControl
{
    public ProfilesPageView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Re-initialize to set behaviors 
        InitializeComponent();

        base.OnAttachedToVisualTree(e);
    }
}
