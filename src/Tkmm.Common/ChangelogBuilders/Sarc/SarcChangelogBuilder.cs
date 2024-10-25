using SarcLibrary;
using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.IO.Buffers;
using Tkmm.Abstractions.Providers;
using Tkmm.Abstractions.Services;
using Tkmm.Common.Extensions;

// ReSharper disable once CheckNamespace
namespace Tkmm.Common.ChangelogBuilders;

internal sealed class SarcChangelogBuilder(IRomfs romfs, IChangelogBuilderProvider provider) : IChangelogBuilder
{
    private readonly IRomfs _romfs = romfs;
    private readonly IChangelogBuilderProvider _provider = provider;

    public async ValueTask LogChanges(string canonical, TkFileAttributes attributes, ArraySegment<byte> input,
        Func<ValueTask<Stream>> getOutput, CancellationToken ct = default)
    {
        if (await _romfs.IsVanilla(input, canonical)) {
            return;
        }

        using RentedBuffer<byte> vanillaBuffer = _romfs.GetVanilla(canonical, attributes);
        Sarc vanilla = Sarc.FromBinary(vanillaBuffer.Segment);
        
        Sarc changelog = [];
        Sarc sarc = Sarc.FromBinary(input);
        
        foreach ((string name, ArraySegment<byte> data) in sarc) {
            if (!vanilla.TryGetValue(name, out ArraySegment<byte> vanillaData)) {
                // Custom file, use entire content
                goto MoveContent;
            }

            string nestedCanonical = Path.GetExtension(canonical.AsSpan()) switch {
                ".pack" => name,
                _ => $"{canonical}/{name}"
            };
            
            if (await _romfs.IsVanilla(data, nestedCanonical)) {
                // Vanilla file, ignore
                continue;
            }

            TkFileInfo nestedFileInfo = nestedCanonical.GetTkFileInfo(string.Empty);
            IChangelogBuilder? builder = _provider.GetChangelogBuilder(nestedFileInfo);
            if (builder is null) {
                goto MoveContent;
            }
            
            await LogChanges(nestedCanonical, nestedFileInfo.Attributes, data,
                () => ValueTask.FromResult<Stream>(sarc.OpenWrite(nestedCanonical)), ct);

        MoveContent:
            changelog[name] = data;
        }

        await using Stream output = await getOutput();
        changelog.Write(output, changelog.Endianness);
    }

    public bool IsKnownFile(in TkFileInfo fileInfo)
    {
        return fileInfo.Extension is ".bfarc" or ".bkres" or ".blarc" or ".genvb" or ".pack" or ".ta";
    }
}