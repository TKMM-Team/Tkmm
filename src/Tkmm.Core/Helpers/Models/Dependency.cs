using System.IO.Compression;
using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Helpers.Models;

public class Dependency
{
    public required string Repo { get; set; }
    public required string Owner { get; set; }
    public required string Tag { get; set; }
    public required Dictionary<string, string> Assets { get; set; }
    public required Dictionary<string, string> Files { get; set; }

    public static string GetOSName()
    {
        return OperatingSystem.IsWindows() ? "win-x64" : "linux-x64";
    }

    public async Task Download()
    {
        string osName = GetOSName();

        string assetName = Assets[osName];
        Stream? stream = await GitHubOperations.GetRelease(Owner, Repo, assetName, Tag);

        string fileName = Files[osName];
        if (Path.GetExtension(assetName) == ".zip") {
            ZipArchive archive = new(stream);
            stream = archive.Entries.Where(x => x.Name == fileName).FirstOrDefault()?.Open();
        }

        string outputFile = Path.Combine(Config.Shared.StaticStorageFolder, "apps", fileName);
        using FileStream fs = File.Create(outputFile);
        stream?.CopyTo(fs);
        stream?.Dispose();
    }
}
