using System.Reflection;
using TkSharp.Core;

namespace Tkmm.Core.IO;

public class EmbeddedSource(string root, Assembly assembly) : ITkSystemSource
{
    public Stream OpenRead(string relativeFilePath)
    {
        return assembly.GetManifestResourceStream($"{root}.{relativeFilePath}")!;
    }

    public bool Exists(string relativeFilePath)
    {
        return assembly.GetManifestResourceInfo($"{root}.{relativeFilePath}") is not null;
    }
}