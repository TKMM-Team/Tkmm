using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Tkmm.Attributes;
using Tkmm.Helpers;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

[Page(Page.Logs, "System Logs", Symbol.AllApps, isFooter: true)]
public partial class LogsPageView : UserControl
{
    public LogsPageView()
    {
        InitializeComponent();
        DataContext = new LogsPageViewModel(this)
            .RegisterListener();
    }
}
