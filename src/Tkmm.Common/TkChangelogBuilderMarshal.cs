using System.Buffers;
using System.Diagnostics;
using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Abstractions.Services;
using Tkmm.Common.Extensions;
using Tkmm.Common.IO;

namespace Tkmm.Common;

/// <summary>
/// Simple marshal over the <see cref="IChangelogBuilderProvider"/> to build changelogs for an <see cref="IModSource"/>.
/// </summary>
/// <param name="changelogBuilderProvider"></param>
public sealed class TkChangelogBuilderMarshal(IZstd zstd, IChangelogBuilderProvider changelogBuilderProvider)
{
    private readonly IZstd _zstd = zstd;
    private readonly IChangelogBuilderProvider _changelogBuilderProvider = changelogBuilderProvider;

    /// <summary>
    /// Builds changelogs for the <see cref="IModSource"/> in parallel.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="writer"></param>
    /// <param name="manifest">The man</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task BuildChangelogs(IModSource source, IModWriter writer, IDictionary<string, ChangelogEntry> manifest, CancellationToken ct = default)
    {
        return Parallel.ForEachAsync(source.RomfsFiles, ct, async (file, cancellationToken) => {
            TkFileInfo fileInfo = file.GetTkFileInfo(source.RomfsPath);
            IChangelogBuilder? builder = _changelogBuilderProvider.GetChangelogBuilder(fileInfo);
            
            if (builder is null) {
                return;
            }
            
            TkFileAttributes attributes = fileInfo.Attributes;
            string canonical = fileInfo.Canonical.ToString();
            
            (Stream input, long streamLength) = await source.OpenRead(file, cancellationToken);
            await BuildChangelog(canonical, attributes, builder, input, streamLength, writer, manifest, cancellationToken);
            await input.DisposeAsync();
        });
    }
    
    private async ValueTask BuildChangelog(string canonical, TkFileAttributes attributes, IChangelogBuilder builder, Stream input, long inputLength, IModWriter writer, IDictionary<string, ChangelogEntry> manifest, CancellationToken ct = default)
    {
        int size = Convert.ToInt32(inputLength);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
        ArraySegment<byte> bufferSegment = new(buffer, 0, size);
        int read = await input.ReadAsync(bufferSegment, ct);
        Debug.Assert(read == size);

        int zsDictionaryId = -1;

        if (IZstd.IsCompressed(bufferSegment)) {
            size = IZstd.GetDecompressedSize(bufferSegment);
            byte[] decompressedBuffer = ArrayPool<byte>.Shared.Rent(size);
            ArraySegment<byte> decompressedBufferSegment = new(decompressedBuffer, 0, size);
            _zstd.Decompress(bufferSegment, decompressedBufferSegment, out zsDictionaryId);
            ArrayPool<byte>.Shared.Return(buffer);
            
            bufferSegment = decompressedBufferSegment;
            buffer = decompressedBuffer;
        }

        try {
            await builder.LogChanges(canonical, attributes, bufferSegment,
                async () => {
                    manifest[canonical] = new ChangelogEntry(ChangelogType.Merge, attributes, zsDictionaryId);
                    return await writer.OpenWrite(canonical);
                }, ct);
        }
        finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}