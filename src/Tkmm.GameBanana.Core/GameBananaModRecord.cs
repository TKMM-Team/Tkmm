using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;

namespace Tkmm.GameBanana.Core;

public partial class GameBananaModRecord : ObservableObject
{
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
    private object? _thumbnail;

    [JsonIgnore]
    public GameBananaMod? Full { get; private set; }

    public async ValueTask DownloadFullMod(CancellationToken ct = default)
    {
        Full = await GameBanana.GetMod(Id, ct);
    }

    public async ValueTask DownloadThumbnail(CancellationToken ct = default)
    {
        if (Media.Images.FirstOrDefault() is not GameBananaImage img || ITkThumbnail.CreateBitmap is null) {
            return;
        }

        await using Stream image = await GameBanana.Get($"{img.BaseUrl}/{img.SmallFile}", ct);
        await using MemoryStream ms = new();
        await image.CopyToAsync(ms, ct);
        ms.Seek(0, SeekOrigin.Begin);
        
        Thumbnail = ITkThumbnail.CreateBitmap(ms);
    }
}
