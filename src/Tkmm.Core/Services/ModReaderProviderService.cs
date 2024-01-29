using Tkmm.Core.Components.ModParsers;

namespace Tkmm.Core.Services;

public static class ModReaderProviderService
{
    private static readonly IModReader[] _parsers = [
        new ZipModReader(),
        new TkclModReader()
    ];

    public static IModReader? GetReader(string file)
    {
        return _parsers.FirstOrDefault(x => x.IsValid(file));
    }
}
