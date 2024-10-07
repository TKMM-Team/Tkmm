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

    public static async ValueTask<object> ShowAsync(Exception ex, params TaskDialogButton[] buttons)
    {
        if (buttons.Length is 0) {
            buttons = [
                TaskDialogButton.OKButton
            ];
        }
        
        TaskDialog dialog = new() {
            XamlRoot = App.XamlRoot,
            Title = $"{ex.GetType().Name.Humanize()}",
            Content = new ErrorDialog() {
                DataContext = ex
            },
            Buttons = buttons
        };

        return await dialog.ShowAsync();
    }
}