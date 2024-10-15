using Microsoft.Extensions.Logging;

namespace Tkmm.Core.Logging;

public class EventLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new EventLogger(categoryName);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}