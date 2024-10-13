using Tkmm.Abstractions;
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

    public ValueTask Merge(ITkProfile profile, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<ITkMod?> Install(object? input, Stream? stream = null, ModContext context = default, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<(Stream Stream, int Size)> OpenModFile(ITkModChangelog target, string manifestFileName, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ITkModChangelog> GetConfiguredOptions(ITkMod target)
    {
        throw new NotImplementedException();
    }
}