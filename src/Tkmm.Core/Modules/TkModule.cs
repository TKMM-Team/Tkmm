using Ninject.Modules;
using Tkmm.Core.Abstractions;

namespace Tkmm.Core.Modules;

public sealed class TkModule : NinjectModule
{
    public override void Load()
    {
        Bind<ITkModManager>()
            .To<TkModManager>()
            .InSingletonScope();
        Bind<ITkModParserManager>()
            .To<TkModParserManager>()
            .InSingletonScope();
    }
}