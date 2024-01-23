using Cocona;
using Tkmm.Core.Components;
using Tkmm.Core.Models.GameBanana;

namespace Tkmm.Core.Commands;

public class ModCommands
{
    private const string GB_MODS_URL = "https://gamebanana.com/mods/";
    private const int GB_TOTK_ID = 7617;

    private const string INSTALL = "Import a mod from a folder, tkcl file, gamebanana mod url, or gamebanana mod id";
    private const string INSTALL_APPLY_OPTION = "Apply the changes to the mod list, only set to false if the app is currently running";

    [Command("import", Aliases = ["add", "install"], Description = INSTALL)]
    public static async Task InstallMod([Argument] string path, [Option(Description = INSTALL_APPLY_OPTION)] bool apply = true)
    {
        if (Path.Exists(path)) {
            ModManager.Shared.Import(path);
            goto Apply;
        }

        string id;
        if (path.StartsWith(GB_MODS_URL)) {
            id = Path.GetRelativePath(GB_MODS_URL, path)
                .Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
        }
        else if (int.TryParse(path, out _)) {
            id = path;
        }
        else {
            throw new InvalidDataException($"""
                Invalid path, url, or mod id: '{path}'
                """);
        }

        GameBananaMod gbMod = await GameBananaMod.Download(id);
        if (gbMod.Game.Id != GB_TOTK_ID) {
            throw new InvalidDataException($"""
                The mod '{gbMod.Name}' is not for TotK
                """);
        }

        if (gbMod.Files.FirstOrDefault(x => x.Name.EndsWith(".zip") || x.Name.EndsWith(".tkcl")) is not GameBananaFile file) {
            throw new InvalidDataException($"""
                The mod '{gbMod.Name}' has no valid zip or tkcl file
                """);
        }

        await gbMod.Install(file);

    Apply:
        if (apply) {
            ModManager.Shared.Apply();
        }
    }

    [Command("list", Aliases = ["ls"], Description = "Print information about each installed mod")]
    public static Task ListMods()
    {
        foreach (var mod in ModManager.Shared.Mods) {
            Console.WriteLine($"""
                - {mod.Name} ({mod.Version}) by {mod.Author}
                """);
        }

        return Task.CompletedTask;
    }
}
