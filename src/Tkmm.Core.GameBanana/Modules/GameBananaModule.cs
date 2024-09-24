using Ninject.Modules;
using Tkmm.Core.Abstractions.Parsers;
using Tkmm.Core.GameBanana.Parsers;

namespace Tkmm.Core.GameBanana.Modules;

public class GameBananaModule : NinjectModule
{
    public override void Load()
    {
        TKMM.DI.Bind<ITkModParser>()
            .To<GameBananaModParser>()
            .InSingletonScope();
    }
}