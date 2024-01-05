using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.Models;

public partial class WikiItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;
}
