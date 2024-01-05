using Octokit;

namespace Tkmm.Core.Helpers.Operations;

public class GitHubOperations
{
    private static readonly GitHubClient _githubClient = new(
        new ProductHeaderValue("Tkmm.Core.Helpers.Operations.GitHubOperations")
    );

    public static async Task<byte[]> GetAsset(string org, string repo, string assetPath)
    {
        return await _githubClient.Repository.Content.GetRawContent(org, repo, assetPath);
    }
}
