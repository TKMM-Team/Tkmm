using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Abstractions;

namespace Tkmm.Core.Models;

public partial class TkModContributor : ObservableObject, ITkModContributor
{
    [ObservableProperty]
    private string _author;
    
    [ObservableProperty]
    private string _contribution;

    public TkModContributor(string author, string contribution)
    {
        
    }
}