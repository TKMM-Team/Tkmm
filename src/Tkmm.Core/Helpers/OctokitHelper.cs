using System.Text.Json;
using Octokit;
using TkSharp.Extensions.GameBanana.Helpers;

namespace Tkmm.Core.Helpers;

public static class OctokitHelper
{
    private const string USER_AGENT = "tkmm-client-application";
    
    private static readonly HttpClient Client = new() {
        DefaultRequestHeaders = {
            { "user-agent", USER_AGENT },
            { "accept", "application/octet-stream" },
            { "X-GitHub-Api-Version", "2022-11-28" },
        }
    };

    private static readonly GitHubClient GithubClient = new(
        productInformation: new ProductHeaderValue(USER_AGENT)
    );
    
    public static Task<Release> GetLatestRelease(string owner, string name)
    {
        return GithubClient
            .Repository
            .Release
            .GetLatest(owner, name);
    }
    
    public static async Task<Stream?> DownloadReleaseAsset(Release release, string assetName, string repo, CancellationToken ct = default)
    {
        var asset = release
            .Assets
            .FirstOrDefault(asset => asset.Name == assetName);
        
        if (asset is null) {
            return null;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/TKMM-Team/{repo}/releases/assets/{asset.Id}");
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        
        var response = await Client.SendAsync(request, ct);
        
        if (!response.IsSuccessStatusCode) {
            return null;
        }
        
        var jsonContent = await response.Content.ReadAsStringAsync(ct);
        using var jsonDoc = JsonDocument.Parse(jsonContent);

        if (!jsonDoc.RootElement.TryGetProperty("digest", out var digestElement)) {
            return null;
        }
        
        var digest = digestElement.GetString();

        if (string.IsNullOrEmpty(digest) || !digest.StartsWith("sha256:")) {
            return null;
        }
        
        var hexHash = digest[7..];
        var expectedHash = Convert.FromHexString(hexHash);
        var buffer = await DownloadHelper.DownloadAndVerify(asset.Url, expectedHash, ChecksumType.Sha256, ct);
        return new MemoryStream(buffer);
    }
}