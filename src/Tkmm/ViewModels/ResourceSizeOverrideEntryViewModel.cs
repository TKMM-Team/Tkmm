using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.ViewModels;

public sealed partial class ResourceSizeOverrideEntryViewModel(string canonical, uint size) : ObservableObject
{
    [ObservableProperty]
    private string _canonical = canonical;

    [ObservableProperty]
    private uint _size = size;
}