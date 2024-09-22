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

    public async ValueTask<ITkModParser?> GetParser(string input)
    {
        foreach (ITkModParser parser in _parsers) {
            if (await parser.CanParseInput(input)) {
                return parser;
            }
        }

        return default;
    }

    public void RegisterParser(ITkModParser parser)
    {
        _parsers.Add(parser);
    }
}