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

    public static ValueTask<object> ShowAsync(Exception ex, params TaskDialogStandardResult[] buttons)
        => ShowAsync(ex, forceShowInDebug: false, buttons);
    
    public static async ValueTask<object> ShowAsync(Exception ex, bool forceShowInDebug = false, params TaskDialogStandardResult[] buttons)
    {
#if DEBUG
        if (!forceShowInDebug) {
            throw ex;
        }
#endif
        
        return await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => {
            if (buttons.Length is 0) {
                buttons = [
                    TaskDialogStandardResult.OK
                ];
            }

            TaskDialog dialog = new() {
                XamlRoot = App.XamlRoot,
                Title = $"{ex.GetType().Name.Humanize(LetterCasing.Title)}",
                Content = new ErrorDialog() {
                    DataContext = ex
                },
                Buttons = [
                    ..
                    buttons.Select(MapToButton)
                ]
            };

            return await dialog.ShowAsync();
        });
    }

    private static TaskDialogButton MapToButton(TaskDialogStandardResult result)
    {
        return result switch {
            TaskDialogStandardResult.Cancel => TaskDialogButton.CancelButton,
            TaskDialogStandardResult.Yes => TaskDialogButton.YesButton,
            TaskDialogStandardResult.No => TaskDialogButton.NoButton,
            TaskDialogStandardResult.Retry => TaskDialogButton.RetryButton,
            TaskDialogStandardResult.Close => TaskDialogButton.CloseButton,
            _ => TaskDialogButton.OKButton
        };
    }
}