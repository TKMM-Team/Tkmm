using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.Models.Mods;

public partial class ModOptionDependency(Guid group, Guid option) : ObservableObject
{
    [ObservableProperty]
    private Guid _group = group;

    [ObservableProperty]
    private Guid _option = option;
}
