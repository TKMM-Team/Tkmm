using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.Models;

public sealed partial class PathCollectionItem : ObservableObject
{
    [ObservableProperty]
    private string _target = string.Empty;
    
    public static implicit operator PathCollectionItem(string target) => new() {
        Target = target
    };
}