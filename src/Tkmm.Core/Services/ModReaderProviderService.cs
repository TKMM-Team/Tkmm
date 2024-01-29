using Tkmm.Core.Components.ModParsers;

namespace Tkmm.Core.Services;

public static class ModReaderProviderService
{
    private static readonly IModReader[] _readers = [
        new ArchiveModReader(),
        new SevenZipModReader(),
        new TkclModReader(),
    ];

    public static IModReader? GetReader(string path)
    {
        return _readers.FirstOrDefault(x => x.IsValid(path));
    }
}
