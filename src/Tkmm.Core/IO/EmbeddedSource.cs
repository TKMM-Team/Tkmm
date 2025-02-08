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

    public string GetAbsolute(string relativeFilePath)
    {
        return string.Create(root.Length + 1 + relativeFilePath.Length, relativeFilePath, (span, chars) => {
            int pos = -1;
            foreach (char @char in root) {
                span[++pos] = @char;
            }

            span[++pos] = '.';

            foreach (char @char in chars) {
                span[++pos] = @char switch {
                    '\\' or '/' => '.',
                    _ => @char
                };
            }
        });
    }
}