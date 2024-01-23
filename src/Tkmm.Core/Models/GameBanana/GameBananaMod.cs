using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Models.GameBanana;

public partial class GameBananaMod : ObservableObject
{
    private const string ENDPOINT = $"/Mod/{{0}}/ProfilePage";

    [JsonPropertyName("_sName")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("_aPreviewMedia")]
    public GameBananaMedia Media { get; set; } = new();

    [JsonPropertyName("_sVersion")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("_bIsPrivate")]
    public bool IsPrivate { get; set; }

    [JsonPropertyName("_bIsFlagged")]
    public bool IsFlagged { get; set; }

    [JsonPropertyName("_bIsTrashed")]
    public bool IsTrashed { get; set; }

    [JsonPropertyName("_aFiles")]
    public List<GameBananaFile> Files { get; set; } = [];

    [JsonPropertyName("_sText")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("_aSubmitter")]
    public GameBananaSubmitter Submitter { get; set; } = new();

    [JsonPropertyName("_aGame")]
    public GameBananaGame Game { get; set; } = new();

    [JsonPropertyName("_aCredits")]
    public List<GameBananaCreditGroup> Credits { get; set; } = [];

    public static async Task<GameBananaMod> Download(string id)
    {
        using Stream stream = await GameBananaHelper.Get(string.Format(ENDPOINT, id));
        return JsonSerializer.Deserialize<GameBananaMod>(stream)
            ?? throw new InvalidOperationException("""
                Could not parse GameBananaMod, the deserializer returned null
                """);
    }

    [RelayCommand]
    public async Task Install(GameBananaFile file)
    {
        using HttpClient client = new();
        using Stream stream = await client.GetStreamAsync(file.DownloadUrl);
        using ZipArchive archive = new(stream);
        string tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        archive.ExtractToDirectory(tmp);

        ObservableCollection<ModContributor> contributors = [];
        foreach (var group in Credits) {
            foreach (var author in group.Authors) {
                contributors.Add(new() {
                    Name = author.Name,
                    Contributions = [author.Role]
                });
            }
        }

        Mod mod = new() {
            Author = Submitter.Name,
            Description = new ReverseMarkdown.Converter().Convert(Text),
            SourceFolder = tmp,
            Name = Name,
            ThumbnailUri = $"{Media.Images[0].BaseUrl}/{Media.Images[0].File}",
            Contributors = contributors,
            Version = Version
        };

        foreach (var folder in Directory.EnumerateDirectories(tmp, "**", SearchOption.AllDirectories)) {
            if (Directory.Exists(Path.Combine(folder, "romfs"))) {
                mod.SourceFolder = folder;
                break;
            }
        }

        PackageGenerator generator = new(mod, ModManager.GetModFolder(mod));
        await generator.Build();

        ModManager.Shared.Mods.Add(mod);
    }
}
