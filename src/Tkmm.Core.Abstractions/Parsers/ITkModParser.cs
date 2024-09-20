namespace Tkmm.Core.Abstractions.Parsers;

public interface ITkModParser
{
    ValueTask<ITkMod> Parse(in Stream metadataStream);
}