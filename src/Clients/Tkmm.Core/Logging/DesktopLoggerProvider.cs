using Microsoft.Extensions.Logging;

namespace Tkmm.Core.Logging;

public class DesktopLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new DesktopLogger(categoryName);
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}