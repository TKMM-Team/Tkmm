using Tkmm.Core.Components.ModReaders;

namespace Tkmm.Core.Services;

public static class ModReaderProviderService
{
    private static readonly IModReader[] _readers = [
        new ArchiveModReader(),
        new FolderModReader(),
        new GameBananaModReader(),
        new ProtocolModReader(),
        new SevenZipModReader(),
        TkclModReader.Instance,
    ];

    public static IModReader? GetReader(string path)
    {
        return _readers.FirstOrDefault(x => x.IsValid(path));
    }
}
