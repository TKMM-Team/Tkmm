using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models;

namespace Tkmm.Core.Components;

public partial class WikiSourceManager : ObservableObject
{
    private static readonly Lazy<WikiSourceManager> _shared = new(() => new());
    public static WikiSourceManager Shared => _shared.Value;

    [ObservableProperty]
    private ObservableCollection<WikiItem> _items = [];

    [RelayCommand]
    public async Task Fetch()
    {
        string cached = Path.Combine(Config.Shared.StorageFolder, "wiki.json");

        // Fill Items
        try {
            byte[] data = await GitHubOperations.GetAsset(
                "TCML-Team", ".github", "/docs/wiki.json");

            Items = JsonSerializer.Deserialize<ObservableCollection<WikiItem>>(data)
                ?? throw new InvalidOperationException("Could not deserialize WikiItems");
            File.WriteAllBytes(cached, data);
        }
        catch (Exception ex) {
            Trace.WriteLine(ex);

            if (File.Exists(cached)) {
                using FileStream fs = File.OpenRead(cached);
                Items = JsonSerializer.Deserialize<ObservableCollection<WikiItem>>(fs)
                    ?? throw new InvalidOperationException("Could not deserialize WikiItems from cache");
            }
        }
    }
}
