namespace Tkmm.Core.Localization;

public class StringResources_Exceptions
{
    private const string GROUP = "Exception";
    
    private readonly string _parserNotFound = GetStringResource(GROUP, nameof(ParserNotFound));
    public Exception ParserNotFound(string argument) => new(
        string.Format(_parserNotFound, argument)
    );
}
