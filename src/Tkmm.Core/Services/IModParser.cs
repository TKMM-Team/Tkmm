using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Services;

public interface IModParser
{
    public Mod Parse(Stream input, string file);
    public bool IsValid(string file);
}
