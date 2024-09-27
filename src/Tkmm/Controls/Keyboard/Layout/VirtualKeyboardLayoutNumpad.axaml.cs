using Avalonia.Controls;
using Tkmm.Controls.Keyboard.Layout;

namespace Tkmm.Controls.Keyboard.Layout;

public partial class VirtualKeyboardLayoutNumpad : KeyboardLayout
{
    public VirtualKeyboardLayoutNumpad()
    {
        InitializeComponent();
    }

    public override string LayoutName => "numpad";
}