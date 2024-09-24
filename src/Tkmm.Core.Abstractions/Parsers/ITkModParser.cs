namespace Tkmm.Core.Abstractions.Parsers;

public interface ITkModParser
{
    bool CanParseInput(string input, CancellationToken ct = default);
    
    ValueTask<ITkMod?> Parse(string input, CancellationToken ct = default);

    ValueTask<ITkMod?> Parse(Stream input, CancellationToken ct = default);
}