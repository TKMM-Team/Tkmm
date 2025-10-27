using Tkmm.Core.IO;
using Tkmm.Core.TkOptimizer;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Core.Services;

public class TkOptimizerService
{
    public static TkOptimizerContext Context { get; } = TkOptimizerContext.Create();

    public static TkChangelog GetMod(TkProfile profile)
    {
        ITkSystemSource? source = new EmbeddedSource(
            "Tkmm.Core.Resources.UltraCam", typeof(TkOptimizerService).Assembly);

        if (!TkOptimizerStore.IsProfileEnabled(profile)) {
            // Only return with cheats when disabled
            goto CheatsOnly;
        }

        Context.Store = TkOptimizerStore.CreateStore(profile);

        var id = GetStaticId();
        if (profile.Mods.FirstOrDefault(x => x.Mod.Id == id) is { } uc) {
            source = uc.Mod.Changelog.Source;
        }

        return new TkChangelog {
            BuilderVersion = 200,
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
        
    CheatsOnly:
        return new TkChangelog {
            BuilderVersion = 200,
            GameVersion = 0,
            CheatFiles = GetCheats(),
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

    public static Ulid GetStaticId()
    {
        Span<byte> idBuffer = stackalloc byte[16];
        idBuffer[^1] = 1;
        return new Ulid(idBuffer);
    }
}