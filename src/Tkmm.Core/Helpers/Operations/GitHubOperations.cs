using Octokit;
using System.Data;

namespace Tkmm.Core.Helpers.Operations;

public class GitHubOperations
{
    private const string USER_AGENT = "Tkmm.Core.Helpers.Operations.GitHubOperations";

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

    public static async Task<(Stream stream, string tag)> GetLatestRelease(string org, string repo, string assetName)
    {
        IReadOnlyList<Release> releases = await _githubClient.Repository.Release.GetAll(org, repo);

        Release? latest = null;
        ReleaseAsset? asset = null;

        int index = -1;
        while (asset == null || latest == null) {
            index++;
            latest = releases[index];
            asset = latest.Assets.Where(x => x.Name == assetName).FirstOrDefault();
        }

        string url = $"https://api.github.com/repos/{org}/{repo}/releases/assets/{asset.Id}";
        return (await _client.GetStreamAsync(url), latest.TagName);
    }

    public static async Task<Stream> GetRelease(string org, string repo, string assetName, string tag)
    {
        Release release = await _githubClient.Repository.Release.Get(org, repo, tag);
        if (release.Assets.Where(x => x.Name == assetName).FirstOrDefault() is ReleaseAsset asset) {
            using HttpClient client = new();
            return await client.GetStreamAsync(asset.BrowserDownloadUrl);
        }

        throw new VersionNotFoundException($"""
            The tag '{tag}' could not be found in the repo '{org}/{repo}'
            """);
    }

    public static async Task<byte[]> GetAsset(string org, string repo, string assetPath)
    {
        return await _githubClient.Repository.Content.GetRawContent(org, repo, assetPath);
    }

    public static async Task<bool> HasUpdate(string org, string repo, string currentTag)
    {
        IReadOnlyList<Release> releases = await _githubClient.Repository.Release.GetAll(org, repo);
        return releases.Count > 0 && releases[0].TagName != currentTag;
    }
}
