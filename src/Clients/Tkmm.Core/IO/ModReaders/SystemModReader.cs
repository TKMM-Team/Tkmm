using System.Text.Json;
using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Models.Mvvm;

namespace Tkmm.Core.IO.ModReaders;

public sealed class SystemModReader : IModReader
{
    public static readonly SystemModReader Instance = new();
    
    public async ValueTask<ITkMod?> ReadMod<T>(T? input, Stream? stream = null, ModContext context = default, CancellationToken ct = default) where T : class
    {
        if (input is not string folder) {
            return null;
        }
        
        string metadata = Path.Combine(folder, "info.json");

        if (!File.Exists(metadata)) {
            return null;
        }
        
        await using FileStream fs = File.OpenRead(metadata);
        return await JsonSerializer.DeserializeAsync(fs, TkJsonContext.Default.TkMod, ct);
    }

    public bool IsKnownInput<T>(T? input) where T : class
    {
        return input is string folder
               && Directory.Exists(folder)
               && Path.GetDirectoryName(folder) == ModManager.SystemModsFolder;
    }
}