using Microsoft.Extensions.Logging;
using Ninject.Modules;
using Tkmm.Core.Abstractions.IO;
using Tkmm.Desktop.IO;

namespace Tkmm.Desktop;

public sealed class DesktopModule : NinjectModule
{
    public override void Load()
    {
        Bind<ILogger>()
            .To<DesktopLogger>()
            .InSingletonScope();
        Bind<ITkFileSystem>()
            .To<DesktopTkFileSystem>()
            .InSingletonScope();
        Bind<IRomfsProvider>()
            .To<DesktopRomfsProvider>()
            .InSingletonScope();
    }
}