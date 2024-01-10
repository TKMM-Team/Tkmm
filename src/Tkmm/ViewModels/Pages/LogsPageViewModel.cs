using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tkmm.Core.Models;

namespace Tkmm.ViewModels.Pages;

public partial class LogsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<SystemLog> _logs = [];
    
    [ObservableProperty]
    private SystemLog? _selected = null;

    [RelayCommand]
    private void Copy()
    {
        if (Selected is null) {
            return;
        }

        Copy(Selected.Message);
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
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            if (desktop.MainWindow?.Clipboard is IClipboard clipboard) {
                clipboard.SetTextAsync(text);
            }
        }
    }

    public LogsPageViewModel RegisterListener()
    {
        Trace.Listeners.Add(new LogsListener(this));
        return this;
    }

    private class LogsListener(LogsPageViewModel vm) : TraceListener
    {
        private const string NULL = "(null)";
        private readonly LogsPageViewModel _vm = vm;

        public override void Write(string? message)
        {
            _vm.Logs.Add(new(message ?? NULL));
        }

        public override void WriteLine(string? message)
            => Write(message);
    }
}
