using System.Net.NetworkInformation;

namespace Tkmm.Core.Helpers;

public static class GameBananaHelper
{
    public static bool IsOnline {
        get {
            try {
                return new Ping()
                    .Send("https://gamebanana.com", 1000).Status == IPStatus.Success;
            }
            catch {
                return false;
            }
        }
    }

    private const string BASE_URL = "https://gamebanana.com/apiv11";
    private static readonly HttpClient _client = new();

    public static async Task<Stream> Get(string endpoint)
    {
        return await _client.GetStreamAsync($"{BASE_URL}{endpoint}");
    }
}
