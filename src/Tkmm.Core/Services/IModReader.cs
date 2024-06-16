using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Services;

public interface IModReader
{
    public bool IsValid(string path);
    public Task<Mod> Read(Stream? input, string path, Guid? modId = null);
}
