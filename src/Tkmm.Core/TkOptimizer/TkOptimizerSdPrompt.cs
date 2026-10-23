namespace Tkmm.Core.TkOptimizer;

public sealed record TkOptimizerSdPathResult(string Path, bool PersistToConfig);

public static class TkOptimizerSdPrompt
{
    public static Func<Task<TkOptimizerSdPathResult?>>? RequestSdCardRootAsync { get; set; }
}
