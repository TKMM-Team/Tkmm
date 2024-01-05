using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tkmm.Models.Mods;

namespace Tkmm.ViewModels.Pages;

public partial class ToolsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ObservableObject> _properties = [
        new ModProperty {
            Name = "Mod Path",
            Watermark = "Mod folder path..."
        },
        new ModImageProperty {
            Name = "Thumbnail Path"
        },
        new ModProperty {
            Name = "Primary Author",
            Watermark = "Primary author name..."
        },
        new ModProperty {
            Name = "Additional Contributors",
            Watermark = "{null}"
        },
        new ModProperty {
            Name = "Mod Title",
            Watermark = "Mod name..."
        },
        new ModProperty {
            Name = "Mod Version",
            Watermark = "Mod version (yours, not the game version)..."
        }
    ];

    [RelayCommand]
    private Task Create()
    {
        throw new NotImplementedException();
    }
}
