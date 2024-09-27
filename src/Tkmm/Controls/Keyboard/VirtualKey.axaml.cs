using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Tkmm.Controls.Keyboard;

public partial class VirtualKey : TemplatedControl
{
    public static readonly StyledProperty<ICommand> ButtonCommandProperty = AvaloniaProperty.Register<VirtualKey, ICommand>(nameof(ButtonCommand));

    public ICommand ButtonCommand {
        get => GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }

    public static readonly StyledProperty<string?> NormalKeyProperty = AvaloniaProperty.Register<VirtualKey, string?>(nameof(NormalKey));

    public string? NormalKey {
        get => GetValue(NormalKeyProperty);
        set => SetValue(NormalKeyProperty, value);
    }

    public static readonly StyledProperty<string?> ShiftKeyProperty = AvaloniaProperty.Register<VirtualKey, string?>(nameof(ShiftKey));

    public string? ShiftKey {
        get => GetValue(ShiftKeyProperty);
        set => SetValue(ShiftKeyProperty, value);
    }

    public static readonly StyledProperty<string?> AltCtrlKeyProperty = AvaloniaProperty.Register<VirtualKey, string?>(nameof(AltCtrlKey));

    public string? AltCtrlKey {
        get => GetValue(AltCtrlKeyProperty);
        set => SetValue(AltCtrlKeyProperty, value);
    }

    public static readonly StyledProperty<object?> CaptionProperty = AvaloniaProperty.Register<VirtualKey, object?>(nameof(Caption));

    public object? Caption {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public static readonly StyledProperty<Key> SpecialKeyProperty = AvaloniaProperty.Register<VirtualKey, Key>(nameof(SpecialKey));

    public Key SpecialKey {
        get => GetValue(SpecialKeyProperty);
        set => SetValue(SpecialKeyProperty, value);
    }

    public static readonly StyledProperty<MaterialIconKind> SpecialIconProperty = AvaloniaProperty.Register<VirtualKey, MaterialIconKind>(nameof(SpecialIcon));

    public MaterialIconKind SpecialIcon {
        get => GetValue(SpecialIconProperty);
        set => SetValue(SpecialIconProperty, value);
    }

    public VirtualKey()
    {
        DataContext = this;

        Initialized += (sender, args) => {
            VirtualKeyboard? keyboard = null;
            if (!Design.IsDesignMode) {
                keyboard = this.GetVisualAncestors().OfType<VirtualKeyboard>().First();

                keyboard.KeyboardStateStream.Subscribe(state => {
                    if (string.IsNullOrEmpty(NormalKey)) {
                        return;
                    }

                    Caption = state switch {
                        VirtualKeyboardState.Default => NormalKey,
                        VirtualKeyboardState.Shift or VirtualKeyboardState.Capslock => ShiftKey,
                        VirtualKeyboardState.AltCtrl => AltCtrlKey,
                        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
                    };
                });

                ButtonCommand = new RelayCommand(() => {
                    if (SpecialKey != Key.None) {
                        keyboard.ProcessKey(SpecialKey);
                    }
                    else {
                        if (Caption is string s && !string.IsNullOrEmpty(s))
                            keyboard.ProcessText(s);
                    }
                });
            }

            if (SpecialKey == Key.LeftShift || SpecialKey == Key.RightShift || SpecialKey == Key.CapsLock || SpecialKey == Key.RightAlt) {
                var toggleButton = new ToggleButton {
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.Parse("Black")),
                    CornerRadius = new CornerRadius(5, 5, 5, 5),
                    Background = new SolidColorBrush(Color.Parse("Black")),
                    Foreground = new SolidColorBrush(Color.Parse("White")),
                    [!WidthProperty] = new Binding("Width"),
                    [!HeightProperty] = new Binding("Height"),
                    [!ContentControl.ContentProperty] = new Binding("Caption"),
                    [!Button.CommandProperty] = new Binding("ButtonCommand"),
                };
                Template = new FuncControlTemplate((control, scope) => toggleButton);

                keyboard?.KeyboardStateStream.Subscribe(state => {
                    switch (state) {
                        case VirtualKeyboardState.Default:
                            toggleButton.IsChecked = false;
                            break;
                        case VirtualKeyboardState.Shift:
                            if (SpecialKey == Key.LeftShift || SpecialKey == Key.RightShift)
                                toggleButton.IsChecked = true;
                            else {
                                toggleButton.IsChecked = false;
                            }

                            break;
                        case VirtualKeyboardState.Capslock:
                            toggleButton.IsChecked = SpecialKey == Key.CapsLock;
                            break;
                        case VirtualKeyboardState.AltCtrl:
                            toggleButton.IsChecked = SpecialKey == Key.RightAlt;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(state), state, null);
                    }
                });
            }
            else {
                Template = new FuncControlTemplate((control, scope) => new Button {
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.Parse("Black")),
                    CornerRadius = new CornerRadius(5, 5, 5, 5),
                    //Background = new SolidColorBrush(Color.Parse("Green")),
                    Foreground = new SolidColorBrush(Color.Parse("White")),
                    [!WidthProperty] = new Binding("Width"),
                    [!HeightProperty] = new Binding("Height"),
                    [!ContentControl.ContentProperty] = new Binding("Caption"),
                    [!Button.CommandProperty] = new Binding("ButtonCommand"),
                });
            }

            if (string.IsNullOrEmpty(NormalKey)) {
                // special cases
                switch (SpecialKey) {
                    case Key.Tab: {
                        var stackPanel = new StackPanel {
                            Orientation = Orientation.Vertical
                        };
                        var first = new MaterialIcon {
                            Kind = SpecialIcon
                        };
                        var second = new MaterialIcon {
                            Kind = SpecialIcon,
                            RenderTransform = new RotateTransform(180.0)
                        };
                        stackPanel.Children.Add(first);
                        stackPanel.Children.Add(second);
                        Caption = stackPanel;
                        IsEnabled = false;
                    }
                        break;
                    case Key.Space: {
                        Caption = null;
                    }
                        break;
                    default:
                        Caption = new MaterialIcon {
                            Kind = SpecialIcon
                        };
                        break;
                }
            }
            else {
                Caption = NormalKey;
            }
        };
    }
}