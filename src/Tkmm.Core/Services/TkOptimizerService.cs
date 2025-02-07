using System.Reflection;
using Tkmm.Core.IO;
using Tkmm.Core.TkOptimizer;
using TkSharp.Core.Models;

namespace Tkmm.Core.Services;

public class TkOptimizerService
{
    public static TkChangelog? GetMod(TkProfile profile) => TkOptimizerStore.IsProfileEnabled(profile) ? GetMod() : null;

    public static TkChangelog GetMod()
    {
        return new TkChangelog {
            BuilderVersion = 100,
            GameVersion = 0,
            ExeFiles = {
                "main.npdm"
            },
            SubSdkFiles = {
                "subsdk3"
            },
            Source = new EmbeddedSource(
                "Tkmm.Resources.UltraCam",
                Assembly.GetCallingAssembly())
        };
    }
}