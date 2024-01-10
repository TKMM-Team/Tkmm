using Avalonia.Controls;
using System.Diagnostics;
using Tkmm.Core.Models;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;
public partial class LogsPageView : UserControl
{
    public LogsPageView()
    {
        InitializeComponent();
        DataContext = new LogsPageViewModel()
            .RegisterListener();

        Trace.WriteLine($"[{LogLevel.Info}] Test 1");
        Trace.WriteLine($"[{LogLevel.Warning}] Test 2");
        Trace.WriteLine($"[{LogLevel.Default}] Test 3");
    }
}
