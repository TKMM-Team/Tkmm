namespace Tkmm.Abstractions.IO;

public interface IPackager
{
    ValueTask<ITkMod> CreatePackage(IModSource source, ModContext context, IModWriter writer);
}