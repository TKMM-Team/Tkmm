using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Tkmm.ViewModels.Pages;
using TkSharp.Packaging;

namespace Tkmm.Views.Pages;

public partial class ProjectsPageView : UserControl
{
    public ProjectsPageView()
    {
        InitializeComponent();
    }

    private void OnResourceSizeTextInput(object? sender, TextInputEventArgs e)
    {
        if (e.Text is { Length: > 0 } text && text.Any(c => !char.IsDigit(c))) {
            e.Handled = true;
        }
    }
}