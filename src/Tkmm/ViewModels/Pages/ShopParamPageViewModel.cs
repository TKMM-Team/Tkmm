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

namespace Tkmm.ViewModels.Pages;

public class Shop
{
    public string ShopName { get; set; }
    public string NPCName { get; set; }
    public string Location { get; set; }
    public string RequiredQuest { get; set; }
    public string NPCActorName { get; set; }
}

public partial class ShopParamPageViewModel : ObservableObject
{
    public ObservableCollection<Shop> Shops { get; set; }

    public ShopParamPageViewModel()
    {
        LoadShops();
    }

    private void LoadShops()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TotK", "shops.json");
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

    public void MoveUp(Shop shop)
    {
        int index = Shops.IndexOf(shop);
        if (index > 0)
        {
            Shops.Move(index, index - 1);
            SaveShops();
        }
    }

    public void MoveDown(Shop shop)
    {
        int index = Shops.IndexOf(shop);
        if (index < Shops.Count - 1)
        {
            Shops.Move(index, index + 1);
            SaveShops();
        }
    }

    private void SaveShops()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TotK", "shops.json");
        string json = JsonSerializer.Serialize(Shops);
        File.WriteAllText(path, json);
    }
}