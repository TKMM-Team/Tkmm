using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Helpers.Models;

public class GithubAsset
{
    private const string LOCALAPPDATA_VAR = "%localappdata%";

    public required string Owner { get; set; }
    public required string Repo { get; set; }
    public required string Asset { get; set; }
    public required string FilePath { get; set; }

    public async Task Download()
    {
        byte[] data = await GitHubOperations.GetAsset(Owner, Repo, Asset);
        string outputFile = Path.GetFullPath(
            Asset.Replace(LOCALAPPDATA_VAR, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
        );

        File.WriteAllBytes(outputFile, data);
    }
}
