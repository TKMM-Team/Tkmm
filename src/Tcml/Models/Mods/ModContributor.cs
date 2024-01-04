using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Tcml.Models.Mods;

public partial class ModContributor : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<string> _contributions = [];
}
