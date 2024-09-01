using CommunityToolkit.HighPerformance;
using System.IO.Compression;
using System.Text.Json;
using Tkmm.Core.Models;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModReaders;

public class TkclModReader : IModReader
{
    internal const int MAGIC = 0x4C434B54;

    internal static readonly TkclVersion Version = new() {
        Major = 1,
        Minor = 0,
        Revision = 0,
    };

    public static readonly TkclModReader Instance = new();

    public bool IsValid(string file)
    {
        return Path.GetExtension(file) == ".tkcl";
    }

    public Task<Mod> Read(Stream? input, string file, Guid? modId)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Read<int>() != MAGIC) {
            throw new InvalidOperationException("""
                Invalid TKCL magic.
                """);
        }

        TkclVersion version = input.Read<TkclVersion>();
        if (version.Value != Version.Value) {
            throw new InvalidOperationException($"""
                Unexpected TKCL version {version}.
                """);
        }

        using ZipArchive archive = new(input, mode: ZipArchiveMode.Read, leaveOpen: true);

        if (archive.Entries.FirstOrDefault(x => x.Name == "info.json")?.Open() is Stream stream) {
            if (JsonSerializer.Deserialize<Mod>(stream) is Mod mod) {
                string outputFolder = Path.Combine(Config.Shared.StorageFolder, "mods", mod.Id.ToString());
                archive.ExtractToDirectory(outputFolder, overwriteFiles: true);
                return Task.FromResult(mod);
            }
        }

        throw new InvalidOperationException("""
            Invalid TKCL file, metadata could not be found.
            """);
    }
}
