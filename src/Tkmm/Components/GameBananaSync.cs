using Tkmm.CLI;
using Tkmm.Core;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.Components;

public static class GameBananaSync
{
    public const string MOD_MANAGER_ALIAS = "TKMM"; // TODO: TBD by GB staff

    public static async Task InstallQueue(CancellationToken ct = default)
    {
        if (GbConfig.Shared is not { PairedSecretKey: { } secretKey, PairedUserId: { } userId }) {
            return;
        }

        var queue = await GameBanana.Get<string[]>(
            $"RemoteInstall/{userId}/{secretKey}/{MOD_MANAGER_ALIAS}", ct: ct);

        if (queue is not null) {
            TkConsoleApp.ProcessArguments(queue.Distinct().ToArray());
        }
    }
}