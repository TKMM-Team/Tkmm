using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using Tkmm.GameBanana.Core.Helpers;

namespace Tkmm.GameBanana.Core;

internal static class GameBanana
{
    private const int MAX_RETRIES = 5;
    
    private const string ROOT = "https://gamebanana.com/apiv11";
    private const string MOD_ENDPOINT = "/Mod/{0}/ProfilePage";
    private const string FEED_ENDPOINT = "/Game/{0}/Subfeed?_nPage={1}&_sSort={2}&_csvModelInclusions=Mod";
    private const string FEED_ENDPOINT_SEARCH = "/Game/{0}/Subfeed?_nPage={1}&_sSort={2}&_sName={3}&_csvModelInclusions=Mod";
    
    public static async ValueTask<Stream> Get(string url, CancellationToken ct = default)
    {
        int attempts = 0;
        
    Retry:
        try {
            attempts++;
            return await DownloadHelper.Client.GetStreamAsync(url, ct);
        }
        catch (HttpRequestException ex) {
            if (ex.StatusCode is HttpStatusCode.BadGateway && attempts < MAX_RETRIES) {
                goto Retry;
            }

            throw;
        }
    }
    
    public static async ValueTask<T?> Get<T>(string path, JsonTypeInfo<T>? typeInfo = null, CancellationToken ct = default)
    {
        return await (typeInfo is not null
            ? DownloadHelper.Client.GetFromJsonAsync($"{ROOT}/{path}", typeInfo, ct)
            : DownloadHelper.Client.GetFromJsonAsync<T>($"{ROOT}/{path}", cancellationToken: ct)
        );
    }

    public static async ValueTask<GameBananaMod?> GetMod(long id, CancellationToken ct = default)
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
        
        GameBananaFeed result = new();

        for (int i = 0; i < 2; i++) {
            GameBananaFeed? feed = await Get(
                endpoint,
                GameBananaFeedJsonContext.Default.GameBananaFeed, ct
            );
            
            if (feed is null) {
                return result;
            }
            
            result.Metadata = feed.Metadata;
            foreach (GameBananaModRecord record in feed.Records) {
                result.Records.Add(record);
            }
        }

        return result;
    }
}