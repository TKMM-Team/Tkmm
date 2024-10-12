using Tkmm.Abstractions.IO;

namespace Tkmm.Abstractions.Providers;

public interface IModWriterProvider
{
    /// <summary>
    /// Get a <see cref="IModWriter"/> that will write content directly into the system.  
    /// </summary>
    /// <returns></returns>
    IModWriter GetSystemWriter();
}