// Copyright (c) Ryujinx Team and Contributors. MIT.
// https://git.ryujinx.app/ryubing/ryujinx/-/blob/master/LICENSE.txt
// 
// Modified by TKMM-Team

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Tkmm.LocaleGenerator;

[Generator]
public class LocaleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var localeFile = context.AdditionalTextsProvider.Where(static x => x.Path.EndsWith("locales.json"));

        var contents = localeFile.Select((text, cancellationToken) => text.GetText(cancellationToken)!.ToString());

        context.RegisterSourceOutput(contents, (spc, content) => {
            var lines = content
                .Split('\n')
                .Where(line => IsKey(line.AsSpan()))
                .Select(GetKey);

            StringBuilder enumSourceBuilder = new();
            enumSourceBuilder.AppendLine("namespace Tkmm.Localization;\n");
            enumSourceBuilder.AppendLine("public enum TkLocale");
            enumSourceBuilder.AppendLine("{");
            
            foreach (var line in lines) {
                enumSourceBuilder.AppendLine($"    {line},");
            }

            enumSourceBuilder.AppendLine("}");

            spc.AddSource("Tkmm.Localization.TkLocale.g.cs", enumSourceBuilder.ToString());
        });
    }

    private static bool IsKey(ReadOnlySpan<char> line)
    {
        if (line.Length < 5) {
            return false;
        }

        return line.Slice(0, 5) is "    \"" && line[line.Length - 1] is '{';
    }

    private static string GetKey(string line)
    {
        var keyEndIndex = line.AsSpan().IndexOf(':');
        return line.Substring(5, keyEndIndex - 6);
    }
}