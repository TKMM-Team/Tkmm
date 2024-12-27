using Avalonia.Controls;
using Tkmm.ViewModels.Wizard;

namespace Tkmm.Views.Wizard;

public partial class WizDumpConfigPage : UserControl
{
    public WizDumpConfigPageViewModel Vm { get; } = new();
    
    public WizDumpConfigPage()
    {
        InitializeComponent();
        DataContext = Vm;
    }
}