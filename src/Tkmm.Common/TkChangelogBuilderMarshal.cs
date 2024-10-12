using System.Buffers;
using System.Diagnostics;
using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Abstractions.Services;
using Tkmm.Common.Extensions;

namespace Tkmm.Common;

public class TkChangelogBuilderMarshal(IChangelogBuilderProvider changelogBuilderProvider)
{
    private readonly IChangelogBuilderProvider _changelogBuilderProvider = changelogBuilderProvider;

    public Task BuildChangelogs(IModSource source, IModWriter writer, CancellationToken ct = default)
    {
        return Parallel.ForEachAsync(source.Files, ct, async (file, cancellationToken) => {
            TkFileInfo fileInfo = file.GetTkFileInfo(source.RomfsPath);
            IChangelogBuilder? builder = _changelogBuilderProvider.GetChangelogBuilder(fileInfo);
            
            if (builder is null) {
                return;
            }
            
            TkFileAttributes attributes = fileInfo.Attributes;
            string canonical = fileInfo.Canonical.ToString();
            
            (Stream input, long streamLength) = await source.OpenRead(file, cancellationToken);
            await BuildChangelog(canonical, attributes, builder, input, streamLength, writer, cancellationToken);
            await input.DisposeAsync();
        });
    }
    
    private static async ValueTask BuildChangelog(string canonical, TkFileAttributes attributes, IChangelogBuilder builder, Stream input, long inputLength, IModWriter writer, CancellationToken ct = default)
    {
        int size = Convert.ToInt32(inputLength);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
        ArraySegment<byte> bufferSegment = new(buffer, 0, size);
        int read = await input.ReadAsync(bufferSegment, ct);
        Debug.Assert(read == size);

        try {
            await builder.LogChanges(canonical, attributes, bufferSegment,
                async () => await writer.OpenWrite(canonical), ct);
        }
        finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}