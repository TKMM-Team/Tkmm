using Tkmm.Core.Abstractions.Parsers;

namespace Tkmm.Core.Abstractions;

public interface ITkModParserManager
{
    ITkModParser GetSystemParser();
    
    ValueTask<ITkModParser?> GetParser(string input);
    
    void RegisterParser(ITkModParser parser);
}