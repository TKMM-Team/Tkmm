using System.Net;
using System.Security.Cryptography;
using System.Web;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModReaders;

internal class ProtocolModReader : IModReader
{
    public bool IsValid(string path)
    {
        return path.Length > 5 && path.AsSpan()[..5] is "tkmm:";
    }

    public async Task<Mod> Read(Stream? input, string path, Guid? modId = null)
    {
        ParseProtocol(path, out byte[] md5Checksum, out string fileUrl, out string modName);
        AppStatus.Set($"Downloading {modName} ({Path.GetFileName(fileUrl)})",
            "fa-regular fa-download", isWorkingStatus: true);

        byte[] data;
        byte[] hash;

        do {
        Retry:
            try {
                using HttpClient client = new() {
                    Timeout = TimeSpan.FromMinutes(2),
                };

                data = await client.GetByteArrayAsync(fileUrl);
                hash = MD5.HashData(data);
            }
            catch (HttpRequestException ex) {
                if (ex.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.RequestTimeout) {
                    goto Retry;
                }

                throw;
            }
            catch {
                throw;
            }
        } while (hash.SequenceEqual(md5Checksum) == false);

        using MemoryStream ms = new(data);
        return await TkclModReader.Instance.Read(ms, fileUrl, modId: default);
    }

    private void ParseProtocol(string path, out byte[] md5Checksum, out string fileUrl, out string modName)
    {
        Span<Range> sections = stackalloc Range[3];
        ReadOnlySpan<char> protocol = path.AsSpan()[5..];
        int foundSections = protocol.Split(sections, ',');

        if (foundSections != 3) {
            throw new ArgumentException(
                $"Invalid TKMM installation link. Expected 'tkmm:{{MD5}},{{FILE_URL}},{{MOD_NAME}}' but received '{path}'",
                nameof(path)
            );
        }

        ReadOnlySpan<char> hash = protocol[sections[0]];
        md5Checksum = Convert.FromHexString(hash);
        fileUrl = HttpUtility.UrlDecode(protocol[sections[1]].ToString());
        modName = HttpUtility.UrlDecode(protocol[sections[2]].ToString());
    }
}
