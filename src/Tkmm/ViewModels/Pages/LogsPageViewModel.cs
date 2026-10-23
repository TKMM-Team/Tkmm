using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Logging;
using Tkmm.Core.Models;

namespace Tkmm.ViewModels.Pages;

public partial class LogsPageViewModel : ObservableObject
{
    public static ObservableCollection<EventLog> Logs => EventLogger.Logs;

    [ObservableProperty]
    public partial EventLog? Selected { get; set; }

    public LogsPageViewModel()
    {
        Logs.CollectionChanged += OnLogsChanged;
    }

    private void OnLogsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Logs.Count == 0) {
            return;
        }

        Dispatcher.UIThread.Post(() => Selected = Logs[^1]);
    }

    [RelayCommand]
    private void Copy()
    {
        if (Selected is null) {
            return;
        }

        Copy(Selected.ToString());
    }

    [RelayCommand]
    private void CopyMarkdown()
    {
        if (Selected is null) {
            return;
        }

        Copy(Selected.ToMarkdown());
    }

    private static void Copy(string text)
    {
        if (App.XamlRoot.Clipboard is { } clipboard) {
            clipboard.SetTextAsync(text);
        }
    }
}
