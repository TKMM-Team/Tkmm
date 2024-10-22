using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Common;
using Tkmm.Common.IO.ModReaders;
using Tkmm.GameBanana.Core.Readers;

namespace Tkmm.Core.Providers;

public class ModReaderProvider : IModReaderProvider
{
    private readonly IModReader[] _readers;

    public ModReaderProvider(TkChangelogBuilderMarshal changelogBuilderMarshal)
    {
        _readers = [
            new GameBananaModReader(this),
            new ArchiveModReader(ModWriterProvider.Instance, changelogBuilderMarshal),
        ];
    }
    
    public IModReader? GetReader<T>(T? input) where T : class
    {
        return _readers
            .FirstOrDefault(reader => reader.IsKnownInput(input));
    }

    public bool CanRead<T>(T? input) where T : class
    {
        return _readers
            .Any(reader => reader.IsKnownInput(input));
    }
}