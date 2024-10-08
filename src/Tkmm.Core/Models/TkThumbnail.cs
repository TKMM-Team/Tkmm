using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Abstractions.Common;

namespace Tkmm.Core.Models;

public partial class TkThumbnail : ObservableObject, IThumbnail
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