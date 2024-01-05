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
    public async Task Fetch(bool forceFetch = false)
    {
        Retry:
        if (!forceFetch) {
            string cached = Path.Combine(Config.Shared.StorageFolder, "wiki.json");
            if (File.Exists(cached)) {
                using FileStream fs = File.OpenRead(cached);
                Items = JsonSerializer.Deserialize<ObservableCollection<WikiItem>>(fs)
                    ?? throw new InvalidOperationException("Could not deserialize WikiItems from cache");
            }

            return;
        }

        // Fill Items
        try {
            byte[] data = await GitHubOperations.GetAsset(
                "TCML-Team", ".github", "/docs/wiki.json");

            Items = JsonSerializer.Deserialize<ObservableCollection<WikiItem>>(data)
                ?? throw new InvalidOperationException("Could not deserialize WikiItems");
        }
        catch (Exception ex) {
            Trace.WriteLine(ex);
            forceFetch = false;
            goto Retry;
        }
    }
}
