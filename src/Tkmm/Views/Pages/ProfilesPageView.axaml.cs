using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Tkmm.Views.Pages;
public partial class ProfilesPageView : UserControl
{
    public ProfilesPageView()
    {
        InitializeComponent();

        // AddHandler(DragDrop.DragOverEvent, DragOver);
        // 
        // MasterListBox.PointerPressed += async (s, e) => {
        //     var mousePos = e.GetPosition(this);
        //     var ghostPos = MasterListBox.Bounds.Position;
        //     var offsetX = mousePos.X - ghostPos.X;
        //     var offsetY = mousePos.Y - ghostPos.Y;
        //     MasterListBox.RenderTransform = new TranslateTransform(offsetX, offsetY);
        // 
        //     var dragData = new DataObject();
        //     var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
        //     Console.WriteLine($"DragAndDrop result: {result}");
        // };
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
