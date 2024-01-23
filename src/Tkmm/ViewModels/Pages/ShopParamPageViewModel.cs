using Avalonia.Controls.PanAndZoom;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using Tkmm.Core;
using Tkmm.Core.Models;

namespace Tkmm.ViewModels.Pages;

public partial class ShopParamPageViewModel : ObservableObject
{
    private static readonly string _shopsFile = Path.Combine(Config.Shared.StaticStorageFolder, "shops.json");

    [ObservableProperty]
    private Shop? _currentShop;

    [ObservableProperty]
    private ObservableCollection<Shop> _shops = [];

    public ShopParamPageViewModel()
    {
        if (File.Exists(_shopsFile)) {
            using FileStream fs = File.OpenRead(_shopsFile);
            Shops = JsonSerializer.Deserialize<ObservableCollection<Shop>>(fs) ?? [];
            CurrentShop = Shops.FirstOrDefault();
        }
    }

    [RelayCommand]
    private Task MoveUp()
    {
        Move(-1);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task MoveDown()
    {
        Move(1);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private static void ResetMap(ZoomBorder zoomBorder)
    {
        zoomBorder.ResetMatrix();
    }

    [RelayCommand]
    private void GotoSelected(ZoomBorder zoomBorder)
    {
        if (CurrentShop is not null) {
            zoomBorder.Zoom(8, CurrentShop.Coordinates.X + 6000, CurrentShop.Coordinates.Y + 5000);
        }
    }

    private void Move(int offset)
    {
        if (CurrentShop is null) {
            return;
        }

        int currentIndex = Shops.IndexOf(CurrentShop);
        int newIndex = currentIndex + offset;

        if (newIndex < 0 || newIndex >= Shops.Count) {
            return;
        }

        Shop store = Shops[newIndex];
        Shops[newIndex] = CurrentShop;
        Shops[currentIndex] = store;

        CurrentShop = Shops[newIndex];
    }

    [RelayCommand]
    private void Apply()
    {
        using FileStream fs = File.Create(_shopsFile);
        JsonSerializer.Serialize(fs, Shops);
    }
}