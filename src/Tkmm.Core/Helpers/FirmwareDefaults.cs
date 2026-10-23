using Tkmm.Core.Models;
using Tkmm.Core.TkOptimizer;

namespace Tkmm.Core.Helpers;

public static class FirmwareDefaults
{
    public static void Apply(SwitchFirmwareVersion firmware)
    {
        if (!firmware.IsFirmware20OrHigher) {
            return;
        }

        TkOptimizerStore.EnableOnAllProfiles();

#if !SWITCH
        Config.Shared.UseRomfslite = true;
#endif
    }
}