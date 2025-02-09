using System.Runtime.InteropServices;
using Octokit;
using TkSharp.Extensions.GameBanana.Helpers;

namespace Tkmm.Core.Helpers;

public static class OctokitHelper
{
    private const string USER_AGENT = "tkmm-client-application";
    
    private static readonly string _runtimeId = OperatingSystem.IsWindows() ? "win" : OperatingSystem.IsLinux() ? "linux" : "osx";
    private static readonly string _assetName = $"Tkmm-{_runtimeId}-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}.zip";
    
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
    
    public static async Task<Stream?> DownloadReleaseAsset(Release release, CancellationToken ct = default)
    {
        ReleaseAsset? md5HashAsset = release
            .Assets
            .FirstOrDefault(asset => asset.Name == _assetName + ".checksum");
        
        ReleaseAsset? asset = release
            .Assets
            .FirstOrDefault(asset => asset.Name == _assetName);

        if (md5HashAsset is null || asset is null) {
            return null;
        }

        await using Stream checksumFile = await _client.GetStreamAsync(asset.Url, ct);
        using StreamReader reader = new(checksumFile);
        if (await reader.ReadLineAsync(ct) is not string md5) {
            return null;
        }
        
        byte[] buffer = await DownloadHelper.DownloadAndVerify(asset.Url, Convert.FromHexString(md5), ct);
        return new MemoryStream(buffer);
    }
}