using Tkmm.Core.Generics;

namespace Tkmm.Core.Services;

public interface IMerger
{
    public Task Merge(IModItem[] mods, string output);
}
