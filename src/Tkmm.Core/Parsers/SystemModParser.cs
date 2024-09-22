using System.Text.Json;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.Parsers;

namespace Tkmm.Core.Parsers;

internal sealed class SystemModParser : ITkModParser
{
    public static readonly SystemModParser Instance = new();
    
    public ValueTask<bool> CanParseInput(string input, CancellationToken ct = default)
    {
        return ValueTask.FromResult(
            // TODO: Remove direct IO usage 
            Directory.Exists(input) && File.Exists(Path.Combine(input, "info.json"))
        );
    }

    public async ValueTask<ITkMod?> Parse(string input, CancellationToken ct = default)
    {
        var metadata = await TKMM.FS.GetMetadata<TkMod>(Path.Combine(input, "info.json"));
        return metadata;
    }

    public async ValueTask<ITkMod?> Parse(Stream input, CancellationToken ct = default)
    {
        var metadata = await JsonSerializer.DeserializeAsync<TkMod>(input, cancellationToken: ct);
        return metadata;
    }
}