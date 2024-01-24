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

    private readonly ZoomBorder _zoomBorder;

    [ObservableProperty]
    private Shop? _currentShop;

    [ObservableProperty]
    private ObservableCollection<Shop> _shops = [];

    public ShopParamPageViewModel(ZoomBorder zoomBorder)
    {
        _zoomBorder = zoomBorder;

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
    private void ResetMap()
    {
        _zoomBorder.ResetMatrix();
    }

    [RelayCommand]
    private void GotoSelected()
    {
        if (CurrentShop is not null) {
            const int zoom = 6;
            _zoomBorder.Zoom(zoom,
                CurrentShop.Coordinates.X + (CurrentShop.Coordinates.X / zoom) + 6000,
                CurrentShop.Coordinates.Y + (CurrentShop.Coordinates.Y / zoom) + 5000
            );
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

    partial void OnCurrentShopChanged(Shop? value)
    {
        GotoSelected();
    }
}