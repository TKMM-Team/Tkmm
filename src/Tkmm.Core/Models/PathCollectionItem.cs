using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.Models;

public sealed partial class PathCollectionItem(PathCollection parent) : ObservableObject
{
    [ObservableProperty]
    private string _target = string.Empty;

    partial void OnTargetChanged(string value)
    {
        parent.EnsureBlankEntry();
    }
}