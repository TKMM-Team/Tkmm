using System.Text.Json;
using Tkmm.Core.Helpers.Models;
using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Helpers;

/// <summary>
/// Manages the assets downloaded from github
/// </summary>
public class AssetHelper
{
    public static async Task<List<GithubAsset>> Load()
    {
        byte[] data = await GitHubOperations.GetAsset("TKMM-Team", ".github", "assets.json");
        return JsonSerializer.Deserialize<List<GithubAsset>>(data)
            ?? throw new InvalidOperationException("""
                Could not parse assets, the JsonDeserializer returned null
                """);
    }

    public static async Task Download()
    {
        AppStatus.Set("Downloading assets", "fa-solid fa-download");

        List<Task> tasks = [];

        foreach (var asset in await Load()) {
            tasks.Add(asset.Download());
        }

        await Task.WhenAll(tasks);
        AppStatus.Set("Assets restored!", "fa-solid fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
    }
}
