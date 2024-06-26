using System.Net;
using System.Security.Cryptography;

namespace Tkmm.Core.Helpers.Operations;

public static class DownloadOperations
{
    public static readonly HttpClient Client = new() {
        Timeout = TimeSpan.FromMinutes(2)
    };

    public static async Task<byte[]> DownloadAndVerify(string fileUrl, byte[] md5Checksum, int maxRetry = 5)
    {
        int retry = 0;
        byte[] data;
        byte[] hash;

        do {
        Retry:
            if (maxRetry < retry) {
                throw new HttpRequestException($"Failed to download resource. The max retry of {maxRetry} was exceeded.",
                    inner: null,
                    HttpStatusCode.BadRequest
                );
            }

            try {
                data = await Client.GetByteArrayAsync(fileUrl);
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
            finally {
                retry++;
            }
        } while (hash.SequenceEqual(md5Checksum) == false);

        return data;
    }
}
