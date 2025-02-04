using FluentAvalonia.UI.Controls;

namespace Tkmm.Dialogs;

public enum MessageDialogButtons
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}

public enum MessageDialogResult
{
    None,
    Ok,
    Yes,
    No,
    Cancel
}

public static class MessageDialog
{
    public static Task<MessageDialogResult> Show(TkLocale content, TkLocale title, MessageDialogButtons buttons = MessageDialogButtons.Ok)
        => Show(Locale[content], Locale[title], buttons);
    
    public static Task<MessageDialogResult> Show(object? content, TkLocale title, MessageDialogButtons buttons = MessageDialogButtons.Ok)
        => Show(content, Locale[title], buttons);
    
    public static async Task<MessageDialogResult> Show(object? content, string? title = null, MessageDialogButtons buttons = MessageDialogButtons.Ok)
    {
        ContentDialog dialog = new() {
            PrimaryButtonText = buttons switch {
                MessageDialogButtons.Ok or MessageDialogButtons.OkCancel => "Ok",
                MessageDialogButtons.YesNo or MessageDialogButtons.YesNoCancel => "Yes",
                _ => string.Empty
            },
            SecondaryButtonText = buttons switch {
                MessageDialogButtons.YesNoCancel => "No",
                _ => string.Empty
            },
            IsSecondaryButtonEnabled = buttons switch {
                MessageDialogButtons.YesNoCancel => true,
                _ => false
            },
            CloseButtonText = buttons switch {
                MessageDialogButtons.YesNo => "No",
                MessageDialogButtons.OkCancel or MessageDialogButtons.YesNoCancel => "Cancel",
                _ => string.Empty
            },
            Title = title,
            Content = content
        };
        
        return (await dialog.ShowAsync(), buttons) switch {
            (ContentDialogResult.Primary, MessageDialogButtons.Ok or MessageDialogButtons.OkCancel) => MessageDialogResult.Ok,
            (ContentDialogResult.Primary, MessageDialogButtons.YesNo or MessageDialogButtons.YesNoCancel) => MessageDialogResult.Yes,
            (ContentDialogResult.Secondary, MessageDialogButtons.YesNoCancel) => MessageDialogResult.No,
            (ContentDialogResult.None, MessageDialogButtons.YesNo) => MessageDialogResult.No,
            (ContentDialogResult.None, MessageDialogButtons.YesNoCancel) => MessageDialogResult.Cancel,
            (_, _) => MessageDialogResult.None
        };
    }
}