using System.Text.RegularExpressions;

namespace Tkmm.Helpers;

public static partial class GameBananaUriHelper
{
    [GeneratedRegex(@"https?://gamebanana\.com/mods/(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex ModUrlRegex();

    [GeneratedRegex(@"https?://gamebanana\.com/members?/(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex MemberUrlRegex();

    public static string ReplaceTkmmUrls(string content)
        => ReplaceMemberUrls(ReplaceModUrls(content));

    private static string ReplaceModUrls(string content)
        => ModUrlRegex().Replace(content, match => $"tkmm://mod/{match.Groups[1].Value}");

    private static string ReplaceMemberUrls(string content)
        => MemberUrlRegex().Replace(content, match => $"tkmm://members/{match.Groups[1].Value}");

    public static string? ToMemberUri(string? url)
    {
        if (string.IsNullOrEmpty(url)) {
            return url;
        }

        return MemberUrlRegex().Match(url) is { Success: true } match
            ? $"tkmm://members/{match.Groups[1].Value}"
            : url;
    }
}
