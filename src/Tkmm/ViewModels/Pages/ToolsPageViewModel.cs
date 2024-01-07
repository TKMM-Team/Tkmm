using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Components;
using Tkmm.Core.Models.Mods;

namespace Tkmm.ViewModels.Pages;

public partial class ToolsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _exportFile = string.Empty;

    [ObservableProperty]
    private Mod _mod = new();

    [RelayCommand]
    private async Task Create()
    {
        AppStatus.Set("Package building...");
        PackageGenerator packageGenerator = new(Mod, ExportFile);
        await packageGenerator.Build();

        AppStatus.Set("Package built");
    }
}
