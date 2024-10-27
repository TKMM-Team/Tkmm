using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.IO.Buffers;
using Tkmm.Common.IO;
using TotkCommon;
using TotkCommon.Components;

namespace Tkmm.Core.IO;

public sealed class ExtractedRomfs : IRomfs
{
    private readonly TkZstd _zstd;
    private static readonly TotkChecksums _checksums;
    public static readonly ExtractedRomfs Instance = new();

    static ExtractedRomfs()
    {
        using Stream stream = typeof(ExtractedRomfs).Assembly.GetManifestResourceStream(
            "Tkmm.Core.Resources.Checksums.bin")!;
        _checksums = TotkChecksums.FromStream(stream);
    }

    public IDictionary<string, string> AddressTable => Totk.AddressTable
        ?? throw new Exception("No address table has been loaded by the romfs implementation.");

    public string Version => Totk.Config.Version.ToString();

    public IZstd Zstd => _zstd;

    public ExtractedRomfs()
    {
        FileInfo zsDicPack = new(Totk.Config.ZsDicPath);
        _zstd = new TkZstd(zsDicPack.OpenRead(), zsDicPack.Length);
    }

    public RentedBuffer<byte> GetVanilla(string fileName, out int zsDictionaryId)
    {
        string absoluteFilePath = Path.Combine(Totk.Config.GamePath, fileName);
        return TkFile.OpenReadAndDecompress(absoluteFilePath, out zsDictionaryId);
    }

    public ValueTask<bool> IsVanilla(in Span<byte> buffer, in ReadOnlySpan<char> canonical, int version)
    {
        return ValueTask.FromResult(
            _checksums.IsFileVanilla(canonical, buffer, version)
        );
    }

    public void LogInfo(ILogger logger)
    {
        logger.LogInformation(
            "[RomFS] Type: Extracted");
        logger.LogInformation(
            "[RomFS] Game Path: {GamePath}", Totk.Config.GamePath);
        logger.LogInformation(
            "[RomFS] Game Version: {GameVersion}", Totk.Config.Version);
        logger.LogInformation(
            "[RomFS] Has ZSTD Dictionaries: {HasZstd}", File.Exists(Totk.Config.ZsDicPath));
    }

    public bool IsStateValid([MaybeNullWhen(true)] out string invalidReason)
    {
        invalidReason = null;

        if (!Directory.Exists(Totk.Config.GamePath)) {
            invalidReason = $"Game path does not exist: '{Totk.Config.GamePath}'.";
            return false;
        }

        if (!File.Exists(Totk.Config.ZsDicPath)) {
            invalidReason = "Incomplete extracted game dump.";
            return false;
        }

        if (Totk.Config.Version <= 100) {
            invalidReason = $"Invalid game version: '{Totk.Config.Version}'.";
            return false;
        }

        return true;
    }
}