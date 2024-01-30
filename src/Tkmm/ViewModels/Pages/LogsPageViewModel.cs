using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tkmm.Core;
using Tkmm.Core.Models;
using Tkmm.Views.Pages;

namespace Tkmm.ViewModels.Pages;

public partial class LogsPageViewModel(LogsPageView view) : ObservableObject
{
    private readonly LogsPageView _view = view;

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

    private class LogsListener : TraceListener
    {
        private const string NULL = "(null)";
        private readonly string _logsPath = Path.Combine(Config.DocumentsFolder, "log.txt");
        private readonly LogsPageViewModel _vm;
        private readonly StreamWriter _writer;

        public LogsListener(LogsPageViewModel vm)
        {
            _vm = vm;

            FileStream fs = File.Create(_logsPath);
            _writer = new(fs) {
                AutoFlush = true
            };
        }

        public override void Write(string? message)
        {
            _vm.Logs.Add(new(message ?? NULL));
            Dispatcher.UIThread.Invoke(_vm._view.Viewer.ScrollToEnd);
            _writer.WriteLine(message);
        }

        public override void WriteLine(string? message)
        {
            Write(message);
        }
    }
}
