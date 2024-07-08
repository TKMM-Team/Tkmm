using Cocona;

namespace Tkmm.Core.Commands;

public class PackageCommands
{
    private const string MOD_FOLDER_NAME = "Mod Folder";
    private const string MOD_FOLDER_DESCRIPTION = "The mod folder to create metadata for.";

    [Command("package", Aliases = ["pack", "pk"])]
    public async Task Package([Argument(MOD_FOLDER_NAME, Description = MOD_FOLDER_DESCRIPTION)] string modFolder)
    {

    }

    [Command("create-metadata", Aliases = ["metadata", "create-info", "info"])]
    public async Task CreateMetadata([Argument(MOD_FOLDER_NAME, Description = MOD_FOLDER_DESCRIPTION)] string modFolder)
    {

    }
}
