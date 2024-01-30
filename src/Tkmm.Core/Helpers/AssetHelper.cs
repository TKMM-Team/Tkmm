using System.Text.Json;
using Tkmm.Core.Helpers.Models;
using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Helpers;

/// <summary>
/// Manages the assets downloaded from github
/// </summary>
public class AssetHelper
{
    private static readonly string _assetsPath = Path.Combine(Config.Shared.StaticStorageFolder, "assets.json");

    public static List<GithubAsset> Assets { get; private set; } = [];

    public static async Task Load(bool forceRefresh = false)
    {
        if (File.Exists(_assetsPath) && !forceRefresh) {
            using FileStream fs = File.OpenRead(_assetsPath);
            Assets = JsonSerializer.Deserialize<List<GithubAsset>>(fs)
                ?? throw new InvalidOperationException("""
                    Could not parse assets, the JsonDeserializer returned null
                    """);
        }

        byte[] data = await GitHubOperations.GetAsset("TKMM-Team", ".github", "assets.json");
        Assets = JsonSerializer.Deserialize<List<GithubAsset>>(data)
            ?? throw new InvalidOperationException("""
                Could not parse assets, the JsonDeserializer returned null
                """);

        using FileStream writer = File.Create(_assetsPath);
        writer.Write(data);
    }

    public static async Task Download()
    {
        AppStatus.Set("Downloading assets", "fa-solid fa-download");

        List<Task> tasks = [];

        foreach (var asset in Assets) {
            tasks.Add(asset.Download());
        }

        await Task.WhenAll(tasks);
        AppStatus.Set("Assets restored!", "fa-solid fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
    }
}
