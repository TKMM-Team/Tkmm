using SarcLibrary;
using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.IO.Buffers;
using Tkmm.Abstractions.Services;
using Tkmm.Common.Extensions;

namespace Tkmm.Common.ChangelogBuilders.Mals;

public class MalsChangelogBuilder(IRomfs romfs) : IChangelogBuilder
{
    private readonly IRomfs _romfs = romfs;

    public async ValueTask LogChanges(string canonical, TkFileAttributes attributes, ArraySegment<byte> input, Func<ValueTask<Stream>> getOutput, CancellationToken ct = default)
    {
        int version = canonical.GetVersionFromCanonical();
        if (await _romfs.IsVanilla(input, canonical, version)) {
            return;
        }
        
        using RentedBuffer<byte> vanillaBuffer = _romfs.GetVanilla(canonical);
        Sarc vanilla = Sarc.FromBinary(vanillaBuffer.Segment);
        
        Sarc changelog = [];
        Sarc sarc = Sarc.FromBinary(input);
    }

    public bool IsKnownFile(in TkFileInfo fileInfo)
    {
        return fileInfo.Extension is ".sarc"
            && fileInfo.Canonical.Length > 4
            && fileInfo.Canonical[..4] is "Mals";
    }
}