using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Components;
using Tkmm.Core.Models.Mods;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;
using Tkmm.Core.Models;
using Tkmm.Core;



namespace Tkmm.ViewModels.Pages;

public partial class ShopParamPageViewModel : ObservableObject
{

    [ObservableProperty]
    private Shop? _currentShop;

    public ObservableCollection<Shop> Shops { get; set; }

    public ShopParamPageViewModel()
    {
        LoadShops();
    }

    private void LoadShops()
    {
        string path = Path.Combine(Config.Shared.StaticStorageFolder, "shops.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Shops = JsonSerializer.Deserialize<ObservableCollection<Shop>>(json);
        }
        else
        {
            Shops = new ObservableCollection<Shop>();
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

    private void Move(int offset)
    {
        if (CurrentShop is null)
        {
            return;
        }

        int currentIndex = Shops.IndexOf(CurrentShop);
        int newIndex = currentIndex + offset;

        if (newIndex < 0 || newIndex >= Shops.Count)
        {
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
        string path = Path.Combine(Config.Shared.StaticStorageFolder, "shops.json");
        string json = JsonSerializer.Serialize(Shops);
        File.WriteAllText(path, json);
    }
}