using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Models.GameBanana;

public partial class GameBananaMod : ObservableObject
{
    private const int GB_TOTK_ID = 7617;
    private const string ENDPOINT = $"/Mod/{{0}}/ProfilePage";

    [JsonPropertyName("_idRow")]
    public ulong Id { get; set; }

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

    public static async Task<Mod> FromId(string id)
    {
        GameBananaMod gamebananaMod = await DownloadMetaData(id);

        if (gamebananaMod.Game.Id != GB_TOTK_ID) {
            throw new InvalidDataException($"""
                The mod '{gamebananaMod.Name}' is not for TotK
                """);
        }

        if (gamebananaMod.Files.FirstOrDefault(x => Path.GetExtension(x.Name) is ".tkcl" or ".zip" or ".rar" or ".7z") is not GameBananaFile file) {
            throw new InvalidDataException($"""
                The mod '{gamebananaMod.Name}' has no valid archive (tkcl, zip, rar, 7z) file
                """);
        }

        return await gamebananaMod.FromFile(file);
    }

    public async Task<Mod> FromFile(GameBananaFile file)
    {
        using HttpClient client = new();
        using Stream stream = await client.GetStreamAsync(file.DownloadUrl);
        Mod mod = await Mod.FromStream(stream, file.Name, BuildModId());

        ObservableCollection<ModContributor> contributors = [];
        foreach (var group in Credits) {
            foreach (var author in group.Authors) {
                contributors.Add(new() {
                    Name = author.Name,
                    Contributions = [author.Role]
                });
            }
        }

        string thumbnailUrl = $"{Media.Images[0].BaseUrl}/{Media.Images[0].File}";
        using Stream thumbnailHttpStream = await client.GetStreamAsync(thumbnailUrl);

        string thumbnailPath = Path.Combine(mod.SourceFolder, PackageBuilder.THUMBNAIL);
        using (FileStream thumbnailFileStream = File.Create(thumbnailPath)) {
            await thumbnailHttpStream.CopyToAsync(thumbnailFileStream);
        }

        mod.Name = Name;
        mod.Author = Submitter.Name;
        mod.Description = new ReverseMarkdown.Converter(new ReverseMarkdown.Config {
            GithubFlavored = true,
            ListBulletChar = '*',
            UnknownTags = ReverseMarkdown.Config.UnknownTagsOption.Bypass
        }).Convert(Text);
        mod.ThumbnailUri = PackageBuilder.THUMBNAIL;
        mod.Contributors = contributors;
        mod.Version = Version;

        PackageBuilder.CreateMetaData(mod, mod.SourceFolder);

        return mod;
    }

    internal static async Task<GameBananaMod> DownloadMetaData(string id)
    {
        using Stream stream = await GameBananaHelper.Get(string.Format(ENDPOINT, id));
        return JsonSerializer.Deserialize<GameBananaMod>(stream)
            ?? throw new InvalidOperationException("""
                Could not parse GameBananaMod, the deserializer returned null
                """);
    }

    private const string GB_GUID = "65c32812-981c-4d7e-8a97-3091ec9c4555";
    private static readonly Guid _gbGuid = Guid.Parse(GB_GUID);

    private Guid BuildModId()
    {
        Span<byte> guidRawBuffer = stackalloc byte[16];
        MemoryMarshal.Write(guidRawBuffer, _gbGuid);
        MemoryMarshal.Write(guidRawBuffer, Id);

        return MemoryMarshal.Read<Guid>(guidRawBuffer);
    }
}
