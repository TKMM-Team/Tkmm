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
    public required List<string> Exclude { get; set; }

    public static string GetOSName()
    {
        return OperatingSystem.IsWindows() ? "win-x64" : "linux-x64";
    }

    public async Task Download()
    {
        string osName = GetOSName();

        string assetName = Assets[osName];
        using Stream stream = await GitHubOperations.GetRelease(Owner, Repo, assetName, Tag);

        string outputFolder = Path.Combine(Config.Shared.StaticStorageFolder, "apps", Repo);

        if (Path.GetExtension(assetName) == ".zip") {
            using ZipArchive archive = new(stream);
            archive.ExtractToDirectory(outputFolder);
            return;
        }

        Directory.CreateDirectory(outputFolder);
        string outputFile = Path.Combine(outputFolder, Files[osName]);
        using FileStream fs = File.Create(outputFile);
        stream.CopyTo(fs);
    }
}
