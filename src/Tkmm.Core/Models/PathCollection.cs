using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.Core.Models;

[JsonConverter(typeof(PathCollectionJsonSerializer))]
public sealed partial class PathCollection : ObservableCollection<PathCollectionItem>, IEnumerable<string>
{
    public PathCollection()
    {
        EnsureBlankEntry();
    }
    
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        EnsureBlankEntry();
    }

    public void New(string target)
    {
        Add(new PathCollectionItem(this) {
            Target = target
        });
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void New()
    {
        Add(new PathCollectionItem(this));
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void Delete(PathCollectionItem target)
    {
        Remove(target);
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => Items.Select(x => x.Target).GetEnumerator();

    public void EnsureBlankEntry()
    {
        if (Count == 0) {
            New();
        }
        
        for (int i = 1; i < Count; i++) {
            if (string.IsNullOrWhiteSpace(this[i].Target)) {
                RemoveAt(i);
                i--;
            }
        }

        if (!string.IsNullOrWhiteSpace(this[0].Target)) {
            Insert(0, new PathCollectionItem(this));
        }
    }
}

public sealed class PathCollectionJsonSerializer : JsonConverter<PathCollection>
{
    public override PathCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        PathCollection result = [];
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray) {
            string value = reader.GetString() ?? string.Empty;
            result.New(value);
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, PathCollection value, JsonSerializerOptions options)
    {
        HashSet<string> tracking = [];
        
        writer.WriteStartArray();
        foreach (PathCollectionItem item in value) {
            if (tracking.Add(item.Target) && !string.IsNullOrWhiteSpace(item.Target)) {
                writer.WriteStringValue(item.Target);
            }
        }
        writer.WriteEndArray();
    }
}