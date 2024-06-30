namespace Tkmm.Core.Helpers;

/// <summary>
/// Helper class to call CLI tools
/// </summary>
public class ExcludeInfo
{
    public static List<string> Folders { get; private set; } = [
        "Mals",
        "RSDB"
    ];

    public static List<string> Extensions { get; private set; } = [
        ".ini",
        ".json",
        ".thumbnail",
        ".rsizetable",
        ".byml",
        ".bgyml",
        ".bfarc",
        ".bkres",
        ".blarc",
        ".genvb",
        ".pack",
        ".ta",
        ".pchtxt"
    ];
}
