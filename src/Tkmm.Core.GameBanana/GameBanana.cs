using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Tkmm.Core.GameBanana;

internal static partial class GameBanana
{
    private const string ROOT = "https://gamebanana.com/apiv11";
    private const string MOD_ENDPOINT = "/Mod/{0}/ProfilePage";
    private const string FEED_ENDPOINT = "/Game/{0}/Subfeed?_nPage={1}&_sSort={2}&_csvModelInclusions=Mod";
    private const string FEED_ENDPOINT_SEARCH = "/Game/{0}/Subfeed?_nPage={1}&_sSort={2}&_sName={3}&_csvModelInclusions=Mod";
    
    private static readonly HttpClient _client = new();
    
    public static async ValueTask<Stream> Get(string url, CancellationToken ct = default)
    {
        return await _client.GetStreamAsync(url, ct);
    }
    
    public static async ValueTask<T?> Get<T>(string path, JsonTypeInfo<T>? typeInfo = null, CancellationToken ct = default)
    {
        return await (typeInfo is not null
            ? _client.GetFromJsonAsync($"{ROOT}/{path}", typeInfo, ct)
            : _client.GetFromJsonAsync<T>($"{ROOT}/{path}", cancellationToken: ct)
        );
    }

    public static async ValueTask<GameBananaMod?> GetMod(long id, CancellationToken ct = default)
    {
        return await Get(
            string.Format(MOD_ENDPOINT, id),
            GameBananaModJsonContext.Default.GameBananaMod, ct
        );
    }

    public static async ValueTask<GameBananaMod?> GetMod(string id, CancellationToken ct = default)
    {
        return await Get(
            string.Format(MOD_ENDPOINT, id),
            GameBananaModJsonContext.Default.GameBananaMod, ct
        );
    }

    public static async ValueTask<GameBananaFeed?> GetFeed(int gameId, int page, string sort, string? searchTerm, CancellationToken ct = default)
    {
        string endpoint = searchTerm switch {
            { Length: > 2 } => string.Format(FEED_ENDPOINT_SEARCH, gameId, page, sort, searchTerm),
            _ => string.Format(FEED_ENDPOINT, gameId, page, sort)
        };
        
        return await Get(
            endpoint,
            GameBananaFeedJsonContext.Default.GameBananaFeed, ct
        );
    }
}
