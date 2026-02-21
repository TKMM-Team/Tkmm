using System.Text.Json;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.Core.Services;

public static class GameBananaRemoteInstallService
{
    public const string MOD_MANAGER_ALIAS = "TotkModManager"; // TODO: TBD by GB staff

    private static readonly Timer _timer = new(e => _ = InstallQueue());

    public static Action<string[]>? ProcessArguments { get; set; }

    public static async Task InstallQueue(CancellationToken ct = default)
    {
        if (GbConfig.Shared is not { PairedSecretKey: { } secretKey, PairedUserId: { } userId }) {
            return;
        }

        var queue = await GameBanana.Get<JsonElement>(
            $"RemoteInstall/{userId}/{secretKey}/{MOD_MANAGER_ALIAS}", ct: ct);

        if (queue.ValueKind is not JsonValueKind.Array) {
            throw new InvalidDataException($"Expected array from GB remote install queue but found: {queue}");
        }

        HashSet<string> mods = new(queue.GetArrayLength());
        foreach (var element in queue.EnumerateArray()) {
            if (element.ValueKind is not JsonValueKind.String || element.GetString() is not { } mod) {
                throw new InvalidDataException($"Expected string, but found: {element}");
            }
            
            mods.Add(mod);
        }

        if (mods.Count > 0) {
            ProcessArguments?.Invoke(mods.ToArray());
        }
    }

    public static void SetInterval(int? minutes)
    {
        if (minutes is null) {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            return;
        }

        _timer.Change(0, TimeSpan.FromMinutes(minutes.Value).Milliseconds);
    }
}