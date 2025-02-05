using System.Text.Json;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Tkmm.Models;

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

/// <summary>
/// Predefined list of dialogs that can be set to never show again.
/// </summary>
public enum MessageDialogs
{
    None,
    CreateExportLocationsDialog
}

public static class MessageDialog
{
    public static readonly HashSet<MessageDialogs> _hiddenDialogs = LoadDialogs();
    
    public static Task<MessageDialogResult> Show(TkLocale content, TkLocale title, MessageDialogButtons buttons = MessageDialogButtons.Ok, MessageDialogs dialog = MessageDialogs.None)
        => Show(Locale[content], Locale[title], buttons, dialog);
    
    public static Task<MessageDialogResult> Show(object? content, TkLocale title, MessageDialogButtons buttons = MessageDialogButtons.Ok, MessageDialogs dialog = MessageDialogs.None)
        => Show(content, Locale[title], buttons, dialog);
    
    public static async Task<MessageDialogResult> Show(object? content, string? title = null, MessageDialogButtons buttons = MessageDialogButtons.Ok, MessageDialogs id = MessageDialogs.None)
    {
        if (_hiddenDialogs.Contains(id)) {
            return MessageDialogResult.None;
        }
        
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
            Title = title
        };

        if (id is not MessageDialogs.None) {
            dialog.Content = new MessageDialogContainer(content);
            dialog.Classes.Add("optional");
        }
        
        MessageDialogResult result = (await dialog.ShowAsync(), buttons) switch {
            (ContentDialogResult.Primary, MessageDialogButtons.Ok or MessageDialogButtons.OkCancel) => MessageDialogResult.Ok,
            (ContentDialogResult.Primary, MessageDialogButtons.YesNo or MessageDialogButtons.YesNoCancel) => MessageDialogResult.Yes,
            (ContentDialogResult.Secondary, MessageDialogButtons.YesNoCancel) => MessageDialogResult.No,
            (ContentDialogResult.None, MessageDialogButtons.YesNo) => MessageDialogResult.No,
            (ContentDialogResult.None, MessageDialogButtons.YesNoCancel) => MessageDialogResult.Cancel,
            (_, _) => MessageDialogResult.None
        };

        if (dialog.Content is MessageDialogContainer { NeverShowAgain: true }) {
            SetHidden(id);
        }

        return result;
    }

    private static readonly string _dialogsStorePath = Path.Combine(AppContext.BaseDirectory, ".layout", "dialogs.json");
    
    public static HashSet<MessageDialogs> LoadDialogs()
    {
        if (!File.Exists(_dialogsStorePath)) {
            return [];
        }

        using FileStream fs = File.OpenRead(_dialogsStorePath);
        return JsonSerializer.Deserialize<HashSet<MessageDialogs>>(fs) ?? [];
    }
    
    public static void SetHidden(MessageDialogs id)
    {
        _hiddenDialogs.Add(id);
        
        if (Path.GetDirectoryName(_dialogsStorePath) is string folder) {
            Directory.CreateDirectory(folder);
        }

        using FileStream fs = File.Create(_dialogsStorePath);
        JsonSerializer.Serialize(fs, _hiddenDialogs);
    }
}