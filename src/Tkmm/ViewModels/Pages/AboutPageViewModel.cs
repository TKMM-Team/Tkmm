using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Components;

namespace Tkmm.ViewModels.Pages;

public partial class AboutPageViewModel : ObservableObject
{
    [RelayCommand]
    private static async Task Refresh()
    {
        await WikiSourceManager.Shared.Fetch(forceFetch: true);
    }
}
