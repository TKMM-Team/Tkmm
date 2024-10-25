using SharpCompress.Archives;
using Tkmm.Abstractions.IO;

namespace Tkmm.Common.IO.ModSources;

public sealed class ArchiveModSource(IArchive archive, IArchiveEntry romfs) : ModSource<IArchiveEntry>(romfs.Key!)
{
    private readonly IArchive _archive = archive;
    private readonly IArchiveEntry _romfs = romfs;

    protected override IEnumerable<IArchiveEntry> RomfsFiles => _archive.Entries
        .Where(entry => !entry.IsDirectory && entry.Key?.StartsWith(_romfs.Key!) is true);

    protected override ValueTask<(Stream Stream, long StreamLength)> OpenRead(IArchiveEntry input, CancellationToken ct = default)
    {
        lock (_archive) {
            Stream stream = input.OpenEntryStream();
            return ValueTask.FromResult((stream, input.Size));
        }
    }

    public override ValueTask<bool> IsKnownInput<TSource>(TSource? input) where TSource : class
    {
        throw new NotImplementedException();
    }

    protected override string GetFileName(IArchiveEntry input)
    {
        return input.Key!;
    }
}