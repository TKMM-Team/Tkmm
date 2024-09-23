namespace Tkmm.Core.GameBanana.Helpers;

public static class GbUrlHelper
{
    public static bool TryGetId(in ReadOnlySpan<char> url, out long id)
    {
        ReadOnlySpan<char> normalizedUrl = url[^1] switch {
            '/' => url[..^1],
            _ => url
        };
        
        id = 0;
        return url.LastIndexOf('/') switch {
            var lastIndex and >= 0 => long.TryParse(normalizedUrl[lastIndex..], out id),
            _ => false
        };
    }
}