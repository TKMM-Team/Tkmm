using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tkmm.Wizard.Pages;

public partial class BaseGameSplitTypePage : UserControl
{
    public BaseGameSplitTypePage()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not BaseGameSplitTypePageContext) {
            throw new InvalidOperationException($"DataContext must be of type {nameof(BaseGameSplitTypePageContext)}");
        }
    }
} 