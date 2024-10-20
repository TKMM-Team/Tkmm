using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;
using Tkmm.Models.Mvvm;

namespace Tkmm.Core;

public partial class TkShopManager : ObservableObject, ITkShopManager
{
    private static readonly string _shopsFile = Path.Combine(AppContext.BaseDirectory, ".persist", "shops.json");

    [ObservableProperty]
    private ITkShop? _selected;

    IList<ITkShop> ITkShopManager.OverflowShops => OverflowShops;

    public ObservableCollection<ITkShop> OverflowShops { get; set; } = [];

    public async Task Initialize(CancellationToken ct = default)
    {
        if (!File.Exists(_shopsFile)) {
            await InitializeFromEmbedded(ct);
            return;
        }
        
        await using Stream? stream = File.OpenRead(_shopsFile);
        await InitializeFromStream(stream, ct);
    }

    private async ValueTask InitializeFromEmbedded(CancellationToken ct = default)
    {
        await using Stream? stream = typeof(TkShopManager).Assembly.GetManifestResourceStream("Tkmm.Core.Resources.Shops.json");
        if (stream is null) {
            return;
        }

        await InitializeFromStream(stream, ct);
    }

    private async ValueTask InitializeFromStream(Stream stream, CancellationToken ct = default)
    {
        if (await JsonSerializer.DeserializeAsync(stream, TkShopJsonContext.Default.ListTkShop, ct) is not { } shops) {
            return;
        }

        foreach (TkShop shop in shops) {
            Selected ??= shop;
            OverflowShops.Add(shop);
        }
    }

    public Task Save(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}

[JsonSerializable(typeof(List<TkShop>))]
internal sealed partial class TkShopJsonContext : JsonSerializerContext;