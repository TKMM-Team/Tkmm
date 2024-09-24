namespace Tkmm.Core.Abstractions.Parsers;

public interface ITkModParser
{
    bool CanParseInput(string input, CancellationToken ct = default);
    
    ValueTask<ITkMod?> Parse(string input, Ulid id = default, CancellationToken ct = default);

    ValueTask<ITkMod?> Parse(Stream input, Ulid id = default, CancellationToken ct = default);
}