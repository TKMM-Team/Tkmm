using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Models.GameBanana;

public partial class GameBananaModInfo : ObservableObject
{
    private const string ENDPOINT = $"/Mod/{{0}}/ProfilePage";

    [JsonPropertyName("_idRow")]
    public int Id { get; set; }

    [JsonPropertyName("_sName")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("_bHasContentRatings")]
    public bool IsContentRated { get; set; }

    [JsonPropertyName("_bIsObsolete")]
    public bool IsObsolete { get; set; }

    [JsonPropertyName("_sProfileUrl")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("_aPreviewMedia")]
    public GameBananaMedia Media { get; set; } = new();

    [JsonPropertyName("_aSubmitter")]
    public GameBananaSubmitter Submitter { get; set; } = new();

    [JsonPropertyName("_sVersion")]
    public string Version { get; set; } = string.Empty;

    [ObservableProperty]
    private object? _thumbnail = null;

    [JsonIgnore]
    public GameBananaMod Info { get; set; } = new();

    [RelayCommand]
    public async Task Install(GameBananaFile file)
    {
        using HttpClient client = new();
        using Stream stream = await client.GetStreamAsync(file.DownloadUrl);
        using ZipArchive archive = new(stream);
        string tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        archive.ExtractToDirectory(tmp);

        Mod mod = new() {
            Author = Submitter.Name,
            Description = new ReverseMarkdown.Converter().Convert(Info.Text),
            SourceFolder = tmp,
            Name = Name,
            ThumbnailUri = $"{Media.Images[0].BaseUrl}/{Media.Images[0].File}",
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

    [RelayCommand]
    public async Task DownloadMod()
    {
        using Stream stream = await GameBananaHelper.Get(string.Format(ENDPOINT, Id.ToString()));
        Info = JsonSerializer.Deserialize<GameBananaMod>(stream)
            ?? throw new InvalidOperationException("""
                Could not deserialize GameBananaMod
                """);
    }
}
