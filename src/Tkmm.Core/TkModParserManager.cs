using Ninject;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.Parsers;
using Tkmm.Core.Parsers;

namespace Tkmm.Core;

internal class TkModParserManager : ITkModParserManager
{
    public ITkModParser GetSystemParser()
    {
        return TKMM.DI.Get<SystemModParser>();
    }

    public ITkModParser GetTkclParser()
    {
        throw new NotImplementedException();
    }

    public ITkModParser? GetParser(string input)
    {
        return TKMM.DI.GetAll<ITkModParser>()
            .FirstOrDefault(parser => parser.CanParseInput(input));
    }

    public bool CanParse(string input)
    {
        return TKMM.DI.GetAll<ITkModParser>()
            .Any(parser => parser.CanParseInput(input));
    }
}