namespace Tkmm.Core.Abstractions.Parsers;

public interface ITkModParserManager
{
    ITkModParser GetSystemParser();
    
    ITkModParser GetParser(string input);
}