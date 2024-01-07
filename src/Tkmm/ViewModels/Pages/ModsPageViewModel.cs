using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Tkmm.Core.Models.Mods;

namespace Tkmm.ViewModels.Pages;

public partial class ModsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Mod> _feed = [];
}
