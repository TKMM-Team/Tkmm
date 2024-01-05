using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Models.Mods;

public partial class ModProperty : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _watermark = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;
}
