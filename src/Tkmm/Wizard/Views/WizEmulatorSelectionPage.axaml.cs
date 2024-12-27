using Avalonia.Controls;

namespace Tkmm.Wizard.Views;

public partial class WizEmulatorSelectionPage : UserControl
{
    public static readonly WizEmulatorSelectionPage Instance = new();
    
    public WizEmulatorSelectionPage()
    {
        InitializeComponent();
    }

    public ValueTask<(bool, int?)> CheckSelection()
    {
        return ValueTask.FromResult<(bool, int?)>((RyujinxOption.IsChecked, SwitchOption.IsChecked, OtherOption.IsChecked) switch {
            (true, false, false) => (true, 0),
            (false, true, false) => (true, 1),
            (false, false, true) => (true, 2),
            _ => (false, null),
        });
    }
}