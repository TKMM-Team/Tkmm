using ConsoleAppFramework;
using Tkmm.CLI;
using Tkmm.Core.Services;

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