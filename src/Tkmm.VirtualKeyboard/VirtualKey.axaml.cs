using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.VirtualKeyboard;

public partial class VirtualKey : ContentControl
{
    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<VirtualKey, ICommand?>(nameof(Command));
    public static readonly StyledProperty<char> KeyProperty = AvaloniaProperty.Register<VirtualKey, char>(nameof(Key));
    public static readonly StyledProperty<char> ShiftKeyProperty = AvaloniaProperty.Register<VirtualKey, char>(nameof(ShiftKey));
    
    public char Key {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public char ShiftKey {
        get => GetValue(ShiftKeyProperty);
        set => SetValue(ShiftKeyProperty, value);
    }

    public ICommand? Command {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    [RelayCommand]
    private void Invoke(VirtualKeyboard keyboard)
    {
        if (Command is not null) {
            Command.Execute(null);
            return;
        }

        bool isShiftEnabled = false;
        
        if (this.FindAncestorOfType<VirtualKeyboardLayout>() is VirtualKeyboardLayout layout) {
            isShiftEnabled = layout.IsShiftEnabled;
        }

        keyboard.VirtualText ??= string.Empty;
        keyboard.VirtualText = keyboard.VirtualText.Insert(keyboard.TextPosition, new string(isShiftEnabled ? ShiftKey : Key, 1));
        keyboard.TextPosition++;
    }
}