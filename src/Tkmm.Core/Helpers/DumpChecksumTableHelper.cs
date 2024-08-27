using Tkmm.Core.Helpers.Operations;
using TotkCommon;

namespace Tkmm.Core.Helpers;

public static class DumpChecksumTableHelper
{
    public static async Task<byte[]> DownloadChecksumTable()
    {
        AppStatus.Set($"Downloading Checksum Table for {Totk.Config.Version}",
            "fa-regular fa-download",
            isWorkingStatus: true
        );

        byte[] result = await GitHubOperations.GetAsset("TKMM-Team", ".github", $"assets/dump-checksums/{Totk.Config.Version}");

        AppStatus.Set($"Checksum Table Successfully Downloaded",
            "fa-regular fa-circle-check",
            isWorkingStatus: false,
            temporaryStatusTime: 2.5
        );

        return result;
    }
}
