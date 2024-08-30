using System.Net.NetworkInformation;

namespace Tkmm.Core.Helpers;

public static class GameBananaHelper
{
    private static readonly Lazy<bool> _isOnline = new(() =>
        {
            try {
                return new Ping()
                    .Send("gamebanana.com", 10000).Status == IPStatus.Success;
            }
            catch {
                return false;
            }
        });


    public static bool IsOnline => _isOnline.Value;

    private const string BASE_URL = "https://gamebanana.com/apiv11";
    private static readonly HttpClient _client = new();

    public static async Task<Stream> Get(string endpoint)
    {
        return await _client.GetStreamAsync($"{BASE_URL}{endpoint}");
    }
}
