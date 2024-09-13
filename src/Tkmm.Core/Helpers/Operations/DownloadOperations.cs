using System.Net;
using System.Security.Cryptography;

namespace Tkmm.Core.Helpers.Operations;

public static class DownloadOperations
{
    public static event Func<IProgress<double>?> OnDownloadStarted = () => null;
    public static event Action OnDownloadCompleted = delegate { };
    
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
                data = await GetBytesAndReportProgress(fileUrl);
                hash = MD5.HashData(data);
            }
            catch (HttpRequestException ex) {
                if (ex.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.RequestTimeout) {
                    goto Retry;
                }

                throw;
            }
            finally {
                retry++;
            }
        } while (hash.SequenceEqual(md5Checksum) == false);

        return data;
    }

    private static async Task<byte[]> GetBytesAndReportProgress(string url)
    {
        using HttpResponseMessage response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength is not { } contentLength) {
            // If the length is not known ahead
            // of time, return the whole buffer
            OnDownloadStarted();
            byte[] staticResult = await response.Content.ReadAsByteArrayAsync();
            OnDownloadCompleted();
            return staticResult;
        }

        const int frameBufferSize = 0x2000;

        IProgress<double>? progress = OnDownloadStarted();
        byte[] result = new byte[contentLength];
        Memory<byte> buffer = result;
        int bytesRead = 0;

        using Stream stream = await response.Content.ReadAsStreamAsync();
        while (bytesRead < contentLength) {
            int nextOffset = (bytesRead + frameBufferSize) >= result.Length
                ? result.Length
                : bytesRead + frameBufferSize;
            int read = await stream.ReadAsync(buffer[bytesRead..nextOffset]);
            bytesRead += read;
            progress?.Report((double)bytesRead / contentLength);
        }

        OnDownloadCompleted();
        return result;
    }
}
