using Avalonia.Controls;

namespace Tkmm.Wizard.Pages;

public partial class BaseGameSplitTypePage : UserControl
{
    public BaseGameSplitTypePage()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not BaseGameSplitTypePageContext) {
            throw new InvalidOperationException($"DataContext must be of type {nameof(BaseGameSplitTypePageContext)}");
        }
    }
} 