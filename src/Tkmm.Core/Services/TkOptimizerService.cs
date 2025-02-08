using Tkmm.Core.IO;
using Tkmm.Core.TkOptimizer;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Core.Services;

public class TkOptimizerService
{
    public static TkOptimizerContext Context { get; } = TkOptimizerContext.Create();

    public static TkChangelog? GetMod(TkProfile? profile = null)
    {
        if (!TkOptimizerStore.IsProfileEnabled(profile)) {
            return null;
        }

        Context.Store = TkOptimizerStore.Attach(profile);

        return new TkChangelog {
            BuilderVersion = 100,
            GameVersion = 0,
            CheatFiles = GetCheats(),
            ExeFiles = {
                "main.npdm"
            },
            SubSdkFiles = {
                "subsdk3"
            },
            Source = new EmbeddedSource(
                "Tkmm.Core.Resources.UltraCam", typeof(TkOptimizerService).Assembly),
        };
    }

    private static List<TkCheat> GetCheats(TkProfile? profile = null)
    {
        Context.Store = TkOptimizerStore.Attach(profile);

        List<TkCheat> result = [
            ..
            Context.CheatGroups.SelectMany(x => x.Cheats
                .Where(optimizerCheat => optimizerCheat.IsEnabled)
                .Select(optimizerCheat => optimizerCheat.TkCheat)
            )
        ];

        Context.Store = null;
        return result;
    }
}