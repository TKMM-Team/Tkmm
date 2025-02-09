using Tkmm.Core.IO;
using Tkmm.Core.TkOptimizer;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Core.Services;

public class TkOptimizerService
{
    public static TkOptimizerContext Context { get; } = TkOptimizerContext.Create();

    public static TkChangelog GetMod(TkProfile? profile = null)
    {
        EmbeddedSource source = new(
            "Tkmm.Core.Resources.UltraCam", typeof(TkOptimizerService).Assembly);

        if (!TkOptimizerStore.IsProfileEnabled(profile)) {
            // Only return with cheats when disabled
            return new TkChangelog {
                BuilderVersion = 100,
                GameVersion = 0,
                CheatFiles = GetCheats(),
                Source = source
            };
        }

        Context.Store = TkOptimizerStore.CreateStore(profile);

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
            Source = source
        };
    }

    private static List<TkCheat> GetCheats(TkProfile? profile = null)
    {
        Context.Store = TkOptimizerStore.CreateStore(profile);

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