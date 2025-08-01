using Octokit;
using TkSharp.Extensions.GameBanana.Helpers;

namespace Tkmm.Core.Helpers;

public static class OctokitHelper
{
    private const string USER_AGENT = "tkmm-client-application";
    
    private static readonly HttpClient _client = new() {
        DefaultRequestHeaders = {
            { "user-agent", USER_AGENT },
            { "accept", "application/octet-stream" },
            { "X-GitHub-Api-Version", "2022-11-28" },
        }
    };

    private static readonly GitHubClient _githubClient = new(
        productInformation: new ProductHeaderValue(USER_AGENT)
    );
    
    public static Task<Release> GetLatestRelease(string owner, string name)
    {
        return _githubClient
            .Repository
            .Release
            .GetLatest(owner, name);
    }
    
    public static async Task<Stream?> DownloadReleaseAsset(Release release, string assetName, CancellationToken ct = default)
    {
        ReleaseAsset? md5HashAsset = release
            .Assets
            .FirstOrDefault(asset => asset.Name == assetName + ".checksum");
        
        ReleaseAsset? asset = release
            .Assets
            .FirstOrDefault(asset => asset.Name == assetName);

        if (md5HashAsset is null || asset is null) {
            return null;
        }

        await using Stream checksumFile = await _client.GetStreamAsync(md5HashAsset.Url, ct);
        using StreamReader reader = new(checksumFile);

        string? md5 = null;
        while (!reader.EndOfStream) {
            string? line = await reader.ReadLineAsync(ct);
            if (!string.IsNullOrWhiteSpace(line)) {
                md5 = line.Trim();
                break;
            }
        }

        if (string.IsNullOrEmpty(md5)) {
            return null;
        }

        byte[] expectedHash = Convert.FromHexString(md5);
        byte[] buffer = await DownloadHelper.DownloadAndVerify(asset.Url, expectedHash, ct);
        return new MemoryStream(buffer);
    }
}