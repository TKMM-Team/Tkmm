using System.IO.Compression;
using System.Text.Json;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components;

public class PackageGenerator
{
    private readonly Mod _mod;
    private readonly string _tempOutput;
    private readonly string _tempRomfsOutput;
    private readonly string _exportFileLocation;

    public PackageGenerator(Mod mod, string tclExportFileLocation)
    {
        _mod = mod;
        _exportFileLocation = tclExportFileLocation;
        _tempOutput = Path.Combine(Path.GetTempPath(), mod.Id.ToString());
        _tempRomfsOutput = Path.Combine(_tempOutput, "romfs");
        Directory.CreateDirectory(_tempRomfsOutput);
    }

    public async Task Build()
    {
        await ToolHelper.Call("MalsMerger", "gen", _mod.SourceFolder, _tempRomfsOutput)
            .WaitForExitAsync();

        if (File.Exists(_mod.ThumbnailUri)) {
            File.Copy(_mod.ThumbnailUri, Path.Combine(_tempOutput, "thumbnail"));
            _mod.ThumbnailUri = "thumbnail";
        }

        using (FileStream fs = File.Create(Path.Combine(_tempOutput, "info.json"))) {
            JsonSerializer.Serialize(fs, _mod);
        }

        if (Path.GetDirectoryName(_exportFileLocation) is string exportFolder) {
            Directory.CreateDirectory(exportFolder);
        }

        using FileStream tcl = File.Create(_exportFileLocation);
        ZipFile.CreateFromDirectory(_tempOutput, tcl);
    }
}
