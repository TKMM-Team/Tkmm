using System.Reflection;
using TkSharp.Core;

namespace Tkmm.Core.IO;

public class EmbeddedSource(string root, Assembly assembly) : ITkSystemSource
{
    public Stream OpenRead(string relativeFilePath)
    {
        return assembly.GetManifestResourceStream(GetAbsolute(relativeFilePath))!;
    }

    public bool Exists(string relativeFilePath)
    {
        return assembly.GetManifestResourceInfo(GetAbsolute(relativeFilePath)) is not null;
    }

    public ITkSystemSource GetRelative(string relativeSourcePath)
    {
        return new EmbeddedSource($"{root}.{relativeSourcePath}", assembly);
    }

    public string GetAbsolute(string relativeFilePath)
    {
        return string.Create(root.Length + 1 + relativeFilePath.Length, relativeFilePath, (span, chars) => {
            var pos = -1;
            foreach (var @char in root) {
                span[++pos] = @char;
            }

            span[++pos] = '.';

            foreach (var @char in chars) {
                span[++pos] = @char switch {
                    '\\' or '/' => '.',
                    _ => @char
                };
            }
        });
    }
}