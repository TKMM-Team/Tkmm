using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.IO.Buffers;
using Tkmm.Abstractions.Providers;
using Tkmm.Abstractions.Services;
using OrderedTarget = (string FileName, Tkmm.Abstractions.ChangelogEntry Entry, System.Collections.Generic.IEnumerable<Tkmm.Abstractions.ITkModChangelog> Mods);

namespace Tkmm.Common;

/// <summary>
/// Simple marshal over the <see cref="IMergerProvider"/> to merge an <see cref="ITkProfile"/>.
/// </summary>
public sealed class TkMergerMarshal(IModManager manager, IMergerProvider mergerProvider, IRomfs romfs)
{
    private readonly IModManager _manager = manager;
    private readonly IMergerProvider _mergerProvider = mergerProvider;
    private readonly IRomfs _romfs = romfs;

    /// <summary>
    /// Merges the provided <see cref="ITkProfile"/> into the <paramref name="mergedOutputWriter"/> <see cref="IModWriter"/>.
    /// </summary>
    /// <param name="profile">The profile to merge.</param>
    /// <param name="mergedOutputWriter">The output mod writer.</param>
    /// <param name="ct"></param>
    public async Task Merge(ITkProfile profile, IModWriter mergedOutputWriter, CancellationToken ct = default)
    {
        IEnumerable<ITkModChangelog> changelogs = profile.Mods
            .Where(profileMod => profileMod.IsEnabled)
            .SelectMany(
                profileMod => _manager
                    .GetConfiguredOptions(profileMod.Mod)
                    .Append(profileMod.Mod)
            )
            .Reverse();

        IEnumerable<OrderedTarget> targets = changelogs
            .SelectMany(
                changelog => changelog.Manifest
                    .Select(kvp => (FileName: kvp.Key, Entry: kvp.Value, Mod: changelog))
            )
            .GroupBy(
                tuple => (
                    tuple.FileName,
                    tuple.Entry
                ),
                tuple => tuple.Mod
            )
            .Select(
                grouping => (
                    grouping.Key.FileName,
                    grouping.Key.Entry,
                    grouping.AsEnumerable()
                )
            );

        TkResourceSizeTable resourceSizeTable = new();

        foreach ((string canonical, ChangelogEntry entry, IEnumerable<ITkModChangelog> mods) in targets) {
            IMerger? merger = _mergerProvider.GetMerger(canonical);
            if (merger is null) {
                continue;
            }

            (Stream Stream, int Size)[] inputs = await Task.WhenAll(
                mods.Select(
                    mod => Task.Run(async () => await _manager.OpenModFile(mod, canonical, ct), ct)
                )
            );

            using RentedBuffers<byte> buffers = await RentedBuffers<byte>.AllocateAsync(inputs, disposeStreams: true, ct);
            using RentedBuffer<byte> vanilla = _romfs.GetVanilla(canonical, entry.Attributes);

            if (!vanilla.Span.IsEmpty) {
                await merger.Merge(buffers, vanilla.Segment,
                    await OpenWriteRomfsOutput(canonical, entry.Attributes, mergedOutputWriter),
                    resourceSizeTable, ct);
                continue;
            }

            await merger.Merge(buffers,
                await OpenWriteRomfsOutput(canonical, entry.Attributes, mergedOutputWriter),
                resourceSizeTable, ct);
        }
        
        // TODO: Merge patches, subsdk and cheats

        await using Stream restblOutputStream = await mergedOutputWriter.OpenWrite(
            Path.Combine("System", "Resource", $"ResourceSizeTable.Product.{_romfs.Version}.rsizetable.zs"));
        await resourceSizeTable.Write(restblOutputStream, ct);
    }

    private ValueTask<Stream> OpenWriteRomfsOutput(string canonical, TkFileAttributes attributes, IModWriter writer)
    {
        string fileName = _romfs.AddressTable.TryGetValue(canonical, out string? versionedFileName)
            ? versionedFileName
            : canonical;

        fileName = attributes.HasFlag(TkFileAttributes.HasZsExtension)
            ? $"romfs{Path.DirectorySeparatorChar}{fileName}.zs"
            : $"romfs{Path.DirectorySeparatorChar}{fileName}";

        return writer.OpenWrite(fileName);
    }
}