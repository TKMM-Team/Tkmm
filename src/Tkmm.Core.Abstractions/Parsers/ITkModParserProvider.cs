namespace Tkmm.Core.Abstractions.Parsers;

public interface ITkModParserProvider
{
    ITkModParser GetSystemParser();
    
    ITkModParser GetParser(string argument);
}