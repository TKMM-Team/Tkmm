using Tkmm.Abstractions;
using Tkmm.Core.IO.ModWriters;
using Tkmm.Models.Mvvm;

namespace Tkmm.Core;

public sealed class ModManager : TkModStorage, IModManager
{
    public static readonly string SystemModsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".system", "mods"); 
    public static readonly string MergedModsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".merged");
    
    public ValueTask Initialize(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask Save(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask Merge(ITkProfile profile, CancellationToken ct = default)
    {
        await TKMM.MergerMarshal.Merge(profile, MergedOutputModWriter.Instance, ct);
    }

    public ValueTask<ITkMod?> Install(object? input, Stream? stream = null, ModContext context = default, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<(Stream Stream, int Size)> OpenModFile(ITkModChangelog target, string manifestFileName, CancellationToken ct = default)
    {
        string targetFile = Path.Combine(SystemModsFolder, target.Id.ToString(), manifestFileName);
        FileInfo targetFileInfo = new(targetFile);
        
        return ValueTask.FromResult<(Stream Stream, int Size)>(
            (Stream: targetFileInfo.OpenRead(), Size: Convert.ToInt32(targetFileInfo.Length))
        );
    }

    public IEnumerable<ITkModChangelog> GetConfiguredOptions(ITkMod target)
    {
        throw new NotImplementedException();
    }
}