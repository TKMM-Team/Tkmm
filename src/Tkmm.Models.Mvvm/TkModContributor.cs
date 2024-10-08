using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;

namespace Tkmm.Models.Mvvm;

public sealed partial class TkModContributor(string author, string contribution) : ObservableObject, ITkModContributor
{
    [ObservableProperty]
    private string _author = author;
    
    [ObservableProperty]
    private string _contribution = contribution;
}