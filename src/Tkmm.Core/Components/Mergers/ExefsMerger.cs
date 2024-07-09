using IPS.NET.Core.Converters;
using System.Text;
using Tkmm.Core.Generics;
using Tkmm.Core.Services;
using TotkCommon;

namespace Tkmm.Core.Components.Mergers;

public class ExefsMerger : IMerger
{
    private const string FLAG_KEYWORD = "@flag";
    private const string ENABLED_KEYWORD = "@enabled";
    private const string STOP_KEYWORD = "@stop";

    private enum State
    {
        None,
        Enabled,
    }

    public Task Merge(IModItem[] mods, string output)
    {
        string[] pchtxtFiles = mods
            .Select(x => Path.Combine(x.SourceFolder, TotkConfig.EXEFS))
            .Where(Directory.Exists)
            .SelectMany(Directory.EnumerateFiles)
            .Where(x => Path.GetExtension(x.AsSpan()) is ".pchtxt")
            .ToArray();

        if (pchtxtFiles.Length <= 0) {
            return Task.CompletedTask;
        }

        State state = State.None;
        string expectedPchtxtHeader = $"@nsobid-{Totk.Config.NSOBID}";
        StringBuilder enabled = new();

        foreach (string file in pchtxtFiles) {
            using FileStream fs = File.OpenRead(file);
            using StreamReader reader = new(fs);

            int lineNumber = 0;

            while (reader.ReadLine() is string line) {
                lineNumber++;

                if (lineNumber == 1) {
                    if (!line.StartsWith(expectedPchtxtHeader, StringComparison.InvariantCultureIgnoreCase)) {
                        goto Skip;
                    }

                    continue;
                }

                if (state is State.Enabled) {
                    if (line.StartsWith(STOP_KEYWORD)) {
                        state = State.None;
                        goto Skip;
                    }

                    enabled.AppendLine(line);
                    continue;
                }

                if (line.StartsWith(ENABLED_KEYWORD)) {
                    state = State.Enabled;
                    continue;
                }
            }

        Skip:
            continue;
        }

        string pchtxt = $"""
            {expectedPchtxtHeader}

            @flag print_values
            @flag offset_shift 0x100

            {ENABLED_KEYWORD}
            {enabled}{STOP_KEYWORD}
            """;

        string outputFolder = Path.Combine(output, TotkConfig.EXEFS);
        Directory.CreateDirectory(outputFolder);

        string outputPath = Path.Combine(outputFolder, $"{Totk.Config.NSOBID}.ips");
        using FileStream ofs = File.Create(outputPath);

        PchtxtToIPS.ConvertPchtxtToIps(pchtxt, ofs);

        return Task.CompletedTask;
    }
}
