using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.Views.Wizard;

public partial class WizPage0 : UserControl
{
    public WizPage0()
    {
        InitializeComponent();
        DataContext = this;
    }

    [RelayCommand]
    private static void StartSetup(ContentControl src)
    {
        src.Content = new WizPage1();
    }
}