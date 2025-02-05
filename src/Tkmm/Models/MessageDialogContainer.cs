using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Models;

public sealed partial class MessageDialogContainer(object? content) : ObservableObject
{
    public object? Content { get; } = content;
    
    [ObservableProperty]
    private bool _neverShowAgain;

    public MessageDialogContainer() : this(null)
    {
    }
}