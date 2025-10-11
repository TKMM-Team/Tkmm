using ConsoleAppFramework;
using TkSharp.Packaging;
using Tkmm.Core;

namespace Tkmm.CLI.Commands;

[RegisterCommands("mods")]
public class ModCommands
{
    public async Task Install([Argument] string arg)
    {
        Console.WriteLine($"Installing {arg}");

        FileStream? fs = null;
        await TKMM.Install(arg, File.Exists(arg) ? fs = File.OpenRead(arg) : null);

        if (fs is not null) {
            await fs.DisposeAsync();
        }
    }

    public async Task Package(
        [Argument] string sourcePath,
        [Argument] string outputPath)
    {
        Console.WriteLine($"Packaging project from {sourcePath} to {outputPath}");

        var project = TkProjectManager.OpenProject(sourcePath);

        await using var output = File.Create(outputPath);
        await project.Package(output, TKMM.GetTkRom());
    }

    public async Task PackageOptimizer(
        [Argument] string sourcePath,
        [Argument] string outputPath)
    {
        Console.WriteLine($"Packaging project from {sourcePath} to {outputPath}");

        var project = TkProjectManager.OpenProject(sourcePath);

        await using var output = File.Create(outputPath);
        await project.PackageOptimizer(output);
    }
}