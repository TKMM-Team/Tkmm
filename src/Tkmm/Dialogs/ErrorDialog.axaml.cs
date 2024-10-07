using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Humanizer;

namespace Tkmm.Dialogs;

public partial class ErrorDialog : UserControl
{
    public ErrorDialog()
    {
        InitializeComponent();
    }

    public static async ValueTask ShowAsync(Exception ex)
    {
        TaskDialog dialog = new() {
            XamlRoot = App.XamlRoot,
            Title = $"{ex.GetType().Name.Humanize()}",
            Content = new ErrorDialog() {
                DataContext = ex
            },
            Buttons = [
                TaskDialogButton.OKButton
            ]
        };

        await dialog.ShowAsync();
    }
}