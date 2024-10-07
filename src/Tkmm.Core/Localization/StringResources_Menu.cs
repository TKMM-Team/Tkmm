namespace Tkmm.Core.Localization;

public class StringResources_Menu
{
    private const string GROUP = "Menu";

    public const string FILE_MENU = nameof(File);
    public string File { get; } = GetStringResource(GROUP, FILE_MENU);

    public const string FILE_INSTALL_FILE = nameof(FileInstallFile);
    public string FileInstallFile { get; } = GetStringResource(GROUP, FILE_INSTALL_FILE);

    public const string FILE_INSTALL_FOLDER = nameof(FileInstallFolder);
    public string FileInstallFolder { get; } = GetStringResource(GROUP, FILE_INSTALL_FOLDER);

    public const string FILE_INSTALL_ARGUMENT = nameof(FileInstallArgument);
    public string FileInstallArgument { get; } = GetStringResource(GROUP, FILE_INSTALL_ARGUMENT);

    public const string FILE_CLEAR_TEMP_FILES = nameof(FileClearTemporaryFiles);
    public string FileClearTemporaryFiles { get; } = GetStringResource(GROUP, FILE_CLEAR_TEMP_FILES);

    public const string FILE_EXIT = nameof(FileExit);
    public string FileExit { get; } = GetStringResource(GROUP, FILE_EXIT);

    public const string MOD_MENU = nameof(Mod);
    public string Mod { get; } = GetStringResource(GROUP, MOD_MENU);

    public const string TOOLS_MENU = nameof(Tools);
    public string Tools { get; } = GetStringResource(GROUP, TOOLS_MENU);

    public const string VIEW_MENU = nameof(View);
    public string View { get; } = GetStringResource(GROUP, VIEW_MENU);

    public const string DEBUG_MENU = nameof(Debug);
    public string Debug { get; } = GetStringResource(GROUP, DEBUG_MENU);

    public const string HELP_MENU = nameof(Help);
    public string Help { get; } = GetStringResource(GROUP, HELP_MENU);
}
