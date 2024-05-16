public partial class VirtualKeyboardLayoutDE : KeyboardLayout
{
    public VirtualKeyboardLayoutDE()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public string LayoutName => "de-DE";
}