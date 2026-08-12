using System.Collections.ObjectModel;

namespace Tkmm.Views.Common;

public partial class JoyConControlsView : OverlayCard
{
    private OverlayModal? _modal;

    public ObservableCollection<JoyConControlMapping> Mappings { get; } = [];

    public JoyConControlsView()
    {
        InitializeComponent();

        JoyConControlMapping[] mappings = [
            new("A", Locale["JoyConControls_Action_Enter"]),
            new("B", Locale["JoyConControls_Action_Backspace"]),
            new("X", Locale["JoyConControls_Action_Escape"]),
            new("L", Locale["JoyConControls_Action_PageUp"]),
            new("R", Locale["JoyConControls_Action_PageDown"]),
            new("ZL", Locale["JoyConControls_Action_RightClick"]),
            new("ZR", Locale["JoyConControls_Action_LeftClick"]),
            new("Left Stick", Locale["JoyConControls_Action_Mouse"]),
            new("Right Stick", Locale["JoyConControls_Action_Scroll"]),
            new("Home", Locale["JoyConControls_Action_Home"]),
            new("Capture", Locale["JoyConControls_Action_Capture"]),
            new("D-Pad Up", Locale["JoyConControls_Action_Up"]),
            new("D-Pad Down", Locale["JoyConControls_Action_Down"]),
            new("D-Pad Left", Locale["JoyConControls_Action_Left"]),
            new("D-Pad Right", Locale["JoyConControls_Action_Right"])
        ];

        foreach (var mapping in mappings) {
            Mappings.Add(mapping);
        }

        DataContext = this;
    }

    public void Show()
    {
        _modal ??= new OverlayModal(this);
        _modal.Show();
    }

    private void Close_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _modal?.Hide();
    }
}

public sealed record JoyConControlMapping(string Button, string Action);