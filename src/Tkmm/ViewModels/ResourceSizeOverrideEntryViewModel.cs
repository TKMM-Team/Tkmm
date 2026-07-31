using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.ViewModels;

public sealed partial class ResourceSizeOverrideEntryViewModel(string canonical, uint size) : ObservableObject
{
    [ObservableProperty]
    public partial string Canonical { get; set; } = canonical;
    [ObservableProperty]
    public partial uint? Size { get; set; } = size;
}