using Tkmm.Core.Abstractions;

namespace Tkmm.Core.Tests;

public class ParserTests : TkTest
{
    [Theory]
    [Trait("Parsers", "Game Banana Parser")]
    [InlineData("https://gamebanana.com/dl/1263600")]
    [InlineData("https://gamebanana.com/dl/1263600/")]
    [InlineData("https://gamebanana.com/mods/510969")]
    [InlineData("https://gamebanana.com/mods/510969/")]
    public async Task GameBananasParser(string input)
    {
        ITkMod? result = await TKMM.ModManager.Create(input);
        result.Should().NotBeNull();
    }
}