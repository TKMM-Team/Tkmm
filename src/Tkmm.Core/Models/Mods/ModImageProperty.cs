using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.Models.Mods;

public partial class ModImageProperty : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;
}
