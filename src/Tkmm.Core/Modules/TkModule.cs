using Ninject.Modules;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.Parsers;
using Tkmm.Core.Parsers;

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
        Bind<ITkModParser>()
            .To<SystemModParser>()
            .InSingletonScope();
    }
}