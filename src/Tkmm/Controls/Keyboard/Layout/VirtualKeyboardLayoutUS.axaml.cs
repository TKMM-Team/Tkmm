using Avalonia.Markup.Xaml;

namespace Tkmm.Controls.Keyboard.Layout;

public partial class VirtualKeyboardLayoutUS : KeyboardLayout
{
    public VirtualKeyboardLayoutUS()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override string LayoutName => "en-US";
}