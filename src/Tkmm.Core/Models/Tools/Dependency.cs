namespace Tkmm.Core.Models.Tools;

public class Dependency
{
    public required string Repo { get; set; }
    public required string Owner { get; set; }
    public required string Tag { get; set; }
    public required Dictionary<string, string> Assets { get; set; }
    public required Dictionary<string, string> Files { get; set; }

    public static string GetOSName()
    {
        return OperatingSystem.IsWindows() ? "win-x64" : "linux-x64";
    }
}
