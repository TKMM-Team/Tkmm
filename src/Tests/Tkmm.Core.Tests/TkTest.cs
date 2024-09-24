using Ninject;
using Tkmm.Core.GameBanana.Modules;
using Tkmm.Core.Modules;
using Tkmm.IO.Desktop.Modules;

namespace Tkmm.Core.Tests;

public class TkTest
{
    private static bool _isSetupCompleted;
    
    protected TkTest()
    {
        if (_isSetupCompleted) {
            return;
        }
        
        TKMM.DI.Load<DesktopModule>();
        TKMM.DI.Load<TkModule>();
        TKMM.DI.Load<GameBananaModule>();
        _isSetupCompleted = true;
    }
}