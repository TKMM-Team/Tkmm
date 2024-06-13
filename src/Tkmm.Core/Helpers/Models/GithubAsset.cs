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
        string outputFile = Path.GetFullPath(
            FilePath.Replace(LOCALAPPDATA_VAR, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
        );

        // Only downlaod shops.json once
        if (Asset == "assets/shops.json" && File.Exists(outputFile)) {
            return;
        }

        if (Path.GetDirectoryName(outputFile) is string folder) {
            Directory.CreateDirectory(folder);
        }

        byte[] data = await GitHubOperations.GetAsset(Owner, Repo, Asset);
        File.WriteAllBytes(outputFile, data);
    }
}
