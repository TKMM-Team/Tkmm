using Tkmm.Core.Generics;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Exceptions;

public class PackageException : Exception
{
    public IModItem? Target { get; }
    public string SystemModFolder { get; }

    public PackageException(IModItem target, string systemModFolder, Exception ex) : base($"Failed to package '{target.Name}'", ex)
    {
        Target = target;
        SystemModFolder = systemModFolder;
    }

    public PackageException(string systemModFolder, Exception ex) : base($"Failed to package '{Path.GetFileName(systemModFolder)}'", ex)
    {
        SystemModFolder = systemModFolder;
    }
}
