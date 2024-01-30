using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Helpers.Models;

public class GithubAsset
{
    public required string Owner { get; set; }
    public required string Repo { get; set; }
    public required string Asset { get; set; }

    public async Task Download()
    {
        byte[] data = await GitHubOperations.GetAsset(Owner, Repo, Asset);
        string outputFile = Path.Combine(Config.Shared.StaticStorageFolder, Path.GetFileName(Asset));
        File.WriteAllBytes(outputFile, data);
    }
}
