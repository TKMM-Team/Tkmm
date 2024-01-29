using Tkmm.Core.Components.ModParsers;

namespace Tkmm.Core.Services;

public static class ModParserService
{
    private static readonly IModParser[] _parsers = [
        new ZipModParser(),
        new TkclModParser()
    ];

    public static IModParser? GetParser(string file)
    {
        return _parsers.FirstOrDefault(x => x.IsValid(file));
    }
}
