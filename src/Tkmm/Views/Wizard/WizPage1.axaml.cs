using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.Views.Wizard;

public partial class WizPage1 : UserControl
{
    public WizPage1()
    {
        InitializeComponent();
        DataContext = this;
    }

    [RelayCommand]
    private static void ChooseEmulator(ContentControl src)
    {
        
    }

    [RelayCommand]
    private static void ChooseSwitchConsole(ContentControl src)
    {
        
    }
}