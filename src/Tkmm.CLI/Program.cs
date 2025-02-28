using ConsoleAppFramework;
using Tkmm.CLI;
using Tkmm.CLI.Logging;
using Tkmm.Core;
using Tkmm.Core.Logging;
using Tkmm.Core.Services;
using TkSharp.Core;

TkLog.Instance.Register(new DesktopLogger(group: nameof(TKMM)));
TkLog.Instance.Register(new ConsoleLogger());

if (!SingleInstanceAppManager.Start(args, Attach)) {
    return;
}

ConsoleApp.Create()
    .Run(args);
    
return;

static void Attach(string[] args)
{
    TkConsoleApp.StartCli(args);
}