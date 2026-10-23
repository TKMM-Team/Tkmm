using System.Text.RegularExpressions;

namespace Tkmm.Helpers;

public static partial class GameBananaUriHelper
{
    [GeneratedRegex(@"https?://gamebanana\.com/mods/(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex ModUrlRegex();

    [GeneratedRegex(@"https?://gamebanana\.com/wips/(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex WipUrlRegex();

    [GeneratedRegex(@"https?://gamebanana\.com/members?/(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex MemberUrlRegex();

    [GeneratedRegex(@"https?://[^\s<>\[\]()]+", RegexOptions.IgnoreCase)]
    private static partial Regex HttpUrlRegex();

    public static string ReplaceTkmmUrls(string content)
        => ReplaceMemberUrls(ReplaceWipUrls(ReplaceModUrls(ReplacePlainTextUrls(content))));

    public static string ReplacePlainTextUrls(string content)
        => HttpUrlRegex().Replace(content, match => {
            var url = match.Value;
            if (match.Index > 0 && content[match.Index - 1] is '(' or '[') {
                return url;
            }

            return $"[{url}]({url})";
        });

    private static string ReplaceModUrls(string content)
        => ModUrlRegex().Replace(content, match => $"tkmm://mod/{match.Groups[1].Value}");

    private static string ReplaceWipUrls(string content)
        => WipUrlRegex().Replace(content, match => $"tkmm://wip/{match.Groups[1].Value}");

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
