using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Revrs.Buffers;
using Tkmm.Core.Abstractions.IO;
using TotkCommon;

namespace Tkmm.IO.Desktop;

public sealed class ExtractedRomfs(ITkFileSystem fs) : IRomfs
{
    public ArraySegmentOwner<byte> GetVanilla(string fileName, out int zsDictionaryId)
    {
        string absoluteFilePath = Path.Combine(Totk.Config.GamePath, fileName);
        return fs.OpenReadAndDecompress(absoluteFilePath, out zsDictionaryId);
    }

    public void LogInfo(ILogger logger)
    {
        logger.LogInformation(
            "[TKMM] [DESKTOP] [IO] [{UtcNow}] RomFS: Extracted", DateTime.UtcNow);
        logger.LogInformation(
            "[TKMM] [DESKTOP] [IO] [{UtcNow}] Game Path: {GamePath}", DateTime.UtcNow, Totk.Config.GamePath);
        logger.LogInformation(
            "[TKMM] [DESKTOP] [IO] [{UtcNow}] Game Version: {GameVersion}", DateTime.UtcNow, Totk.Config.Version);
        logger.LogInformation(
            "[TKMM] [DESKTOP] [IO] [{UtcNow}] Has ZSTD Dictionaries: {HasZstd}", DateTime.UtcNow, File.Exists(Totk.Config.ZsDicPath));
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