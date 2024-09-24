using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.Parsers;
using Tkmm.Core.Parsers;

namespace Tkmm.Core;

internal class TkModParserManager : ITkModParserManager
{
    private static readonly List<ITkModParser> _parsers = [];
    
    public ITkModParser GetSystemParser()
    {
        return SystemModParser.Instance;
    }

    public ITkModParser GetTkclParser()
    {
        throw new NotImplementedException();
    }

    public ITkModParser? GetParser(string input)
    {
        return _parsers.FirstOrDefault(parser => parser.CanParseInput(input));
    }

    public bool CanParse(string input)
    {
        return _parsers.Any(parser => parser.CanParseInput(input));
    }

    public void RegisterParser(ITkModParser parser)
    {
        _parsers.Add(parser);
    }
}