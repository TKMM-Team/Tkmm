using Avalonia.Controls;
using Tkmm.Components.Wizard;

namespace Tkmm.Views.Wizard;

public partial class WizPage : UserControl
{
    public WizPage()
    {
        InitializeComponent();
        DataContext = WizLayout.FirstPage;
    }
}