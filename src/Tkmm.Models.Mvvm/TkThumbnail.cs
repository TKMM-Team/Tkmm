using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;

namespace Tkmm.Models.Mvvm;

public sealed partial class TkThumbnail : ObservableObject, ITkThumbnail
{
    public const string DEFAULT = ".thumbnail";
    
    [ObservableProperty]
    private string _thumbnailPath = DEFAULT;
    
    [ObservableProperty]
    [property: JsonIgnore]
    private object? _bitmap;

    [JsonIgnore]
    public bool IsResolved { get; set; }
}