using Tkmm.Core.Abstractions.Parsers;

namespace Tkmm.Core.Abstractions;

public interface ITkModParserManager
{
    ITkModParser GetSystemParser();
    
    ITkModParser GetTkclParser();
    
    ITkModParser? GetParser(string input);
    
    bool CanParse(string input);
    
    void RegisterParser(ITkModParser parser);
}