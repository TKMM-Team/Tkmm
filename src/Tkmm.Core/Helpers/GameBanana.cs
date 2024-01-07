namespace Tkmm.Core.Helpers;

public static class GameBanana
{
    private const string BASE_URL = "https://gamebanana.com/apiv11";
    private static readonly HttpClient _client = new();

    public static async Task<Stream> Get(string endpoint)
    {
        return await _client.GetStreamAsync($"{BASE_URL}{endpoint}");
    }
}
