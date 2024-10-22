using SharpCompress.Archives;
using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Common.IO.ModSources;
using Tkmm.Models.Mvvm;

namespace Tkmm.Common.IO.ModReaders;

public sealed class ArchiveModReader(IModWriterProvider writerProvider, TkChangelogBuilderMarshal changelogBuilderMarshal) : IModReader
{
    private readonly IModWriterProvider _writerProvider = writerProvider;
    private readonly TkChangelogBuilderMarshal _changelogBuilderMarshal = changelogBuilderMarshal;

    public async ValueTask<ITkMod?> ReadMod<T>(T? input, Stream? stream = null, ModContext context = default, CancellationToken ct = default) where T : class
    {
        if (input is not string fileName || stream is null) {
            return null;
        }
        
        using IArchive archive = ArchiveFactory.Open(stream);
        if (LocateRoot(archive) is not { Key: not null } root) {
            return null;
        }

        if (context.Id == Ulid.Empty) {
            context.Id = Ulid.NewUlid();
        }
        
        ArchiveModSource source = new(archive, root);
        IModWriter writer = _writerProvider.GetSystemWriter(context);

        Dictionary<string, ChangelogEntry> manifest = [];
        await _changelogBuilderMarshal.BuildChangelogs(source, writer, manifest, ct);

        return new TkMod {
            Id = context.Id,
            Name = Path.GetFileNameWithoutExtension(fileName),
            Manifest = manifest
        };
    }

    public bool IsKnownInput<T>(T? input) where T : class
    {
        return input is string path &&
               Path.GetExtension(path.AsSpan()) is ".zip" or ".rar" or ".7z";
    }

    private static IArchiveEntry? LocateRoot(IArchive archive)
    {
        foreach (IArchiveEntry entry in archive.Entries) {
            if (!entry.IsDirectory) {
                continue;
            }

            ReadOnlySpan<char> key = entry.Key.AsSpan();
            if (key.Length > 5 && Path.GetFileName(key[^1] is '/' or '\\' ? key[..^1] : key) is "romfs" or "exefs" or "cheats") {
                return entry;
            }
        }

        return null;
    }
}