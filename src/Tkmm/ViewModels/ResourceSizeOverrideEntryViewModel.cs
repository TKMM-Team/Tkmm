using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.ViewModels;

public sealed partial class ResourceSizeOverrideEntryViewModel(string canonical, uint size) : ObservableObject
{
    public string Canonical { get; } = canonical;

    [ObservableProperty]
    private uint _size = size;
}