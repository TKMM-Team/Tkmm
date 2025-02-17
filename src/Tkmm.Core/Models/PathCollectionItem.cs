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

    public override bool Equals(object? obj)
    {
        if (obj is not PathCollectionItem item) {
            return false;
        }

        return OperatingSystem.IsWindows()
            ? item.Target.Equals(Target, StringComparison.InvariantCultureIgnoreCase)
            : item.Target.Equals(Target);
    }

    private bool Equals(PathCollectionItem other)
    {
        return Target == other.Target;
    }

    public override int GetHashCode()
    {
        return Target.GetHashCode();
    }
}