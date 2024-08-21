using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Tkmm.Attributes;
using Tkmm.Helpers;

namespace Tkmm.Views.Pages;

[Page(Page.Profiles, "Manage mod profiles", Symbol.OtherUser)]
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

    private void DragOver(object? sender, DragEventArgs e)
    {
        var mousePos = e.GetPosition(this);
        var ghostPos = MasterListBox.Bounds.Position;
        var offsetX = mousePos.X - ghostPos.X;
        var offsetY = mousePos.Y - ghostPos.Y;
        MasterListBox.RenderTransform = new TranslateTransform(offsetX, offsetY);
    }

    private void Drop(object? sender, DragEventArgs e)
    {

    }
}
