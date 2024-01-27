using Octokit;
using System.Data;

namespace Tkmm.Core.Helpers.Operations;

public class GitHubOperations
{
    private static readonly GitHubClient _githubClient = new(
        new ProductHeaderValue("Tkmm.Core.Helpers.Operations.GitHubOperations")
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

        using HttpClient client = new();
        return (await client.GetStreamAsync(asset.BrowserDownloadUrl), latest.TagName);
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
