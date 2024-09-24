using System.Text.Json;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.Parsers;

namespace Tkmm.Core.Parsers;

internal sealed class SystemModParser : ITkModParser
{
    public bool CanParseInput(string input, CancellationToken ct = default)
    {
        return 
            // TODO: Remove direct IO usage 
            Directory.Exists(input) && File.Exists(Path.Combine(input, "info.json"));
    }

    public async ValueTask<ITkMod?> Parse(string input, Ulid id = default, CancellationToken ct = default)
    {
        var metadata = await TKMM.FS.GetMetadata<TkMod>(Path.Combine(input, "info.json"));
        
        // TODO: load options
        
        if (metadata is not null) {
            metadata.Id = id;
        }
        
        return metadata;
    }

    public async ValueTask<ITkMod?> Parse(Stream input, Ulid id = default, CancellationToken ct = default)
    {
        var metadata = await JsonSerializer.DeserializeAsync<TkMod>(input, cancellationToken: ct);
        
        // TODO: load options

        if (metadata is not null) {
            metadata.Id = id;
        }
        
        return metadata;
    }
}