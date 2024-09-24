using System.Runtime.CompilerServices;
using ReverseMarkdown;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.Parsers;
using Tkmm.Core.GameBanana.Helpers;
using Tkmm.Core.Models;

namespace Tkmm.Core.GameBanana.Parsers;

public class GameBananaModParser(ITkModParserManager parserManager) : ITkModParser
{
    private readonly ITkModParserManager _parserManager = parserManager;

    public bool CanParseInput(string input, CancellationToken ct)
    {
        return (
            input.Contains("gamebanana.com/mods/") || input.Contains("gamebanana.com/dl/")
        ) && GbUrlHelper.TryGetId(input, out _);
    }

    public async ValueTask<ITkMod?> Parse(string input, Ulid _, CancellationToken ct)
    {
        if (!GbUrlHelper.TryGetId(input, out long id)) {
            return null;
        }

        if (input.Contains("/mods/")) {
            return await ParseFromModId(id, ct);
        }

        return await ParseFromFileUrl(input, id, ct: ct);
    }

    public ValueTask<ITkMod?> Parse(Stream input, Ulid _, CancellationToken ct)
    {
        throw new NotSupportedException(
            "GameBananaModParser does not support parsing from a stream.");
    }
    
    public async ValueTask<ITkMod?> ParseFromModId(long modId, CancellationToken ct)
    {
        GameBananaMod? gbMod = await GameBanana.GetMod(modId, ct);
        GameBananaFile? target = gbMod?.Files
            .FirstOrDefault(x => x.IsTkcl);

        target ??= gbMod?.Files
            .FirstOrDefault(x => _parserManager.CanParse(x.Name));

        if (target is null || gbMod is null) {
            return null;
        }

        ITkMod? mod = await ParseFromFileUrl(target.DownloadUrl, target.Id, target, ct);

        if (mod is null) {
            return null;
        }
        
        mod.Name = gbMod.Name;
        mod.Author = gbMod.Submitter.Name;
        mod.Description = new Converter(new ReverseMarkdown.Config {
            GithubFlavored = true,
            ListBulletChar = '*',
            UnknownTags = ReverseMarkdown.Config.UnknownTagsOption.Bypass
        }).Convert(gbMod.Text);
        mod.Thumbnail = new TkThumbnail();
        mod.Version = gbMod.Version;
        
        foreach (GameBananaAuthor author in gbMod.Credits.SelectMany(group => group.Authors)) {
            mod.Contributors.Add(new TkModContributor(author.Name, author.Role));
        }
        
        return mod;
    }
    
    public async ValueTask<ITkMod?> ParseFromFileUrl(string fileUrl, long fileId, GameBananaFile? target = default, CancellationToken ct = default)
    {
        target ??= await GameBanana.Get($"File/{fileId}", GameBananaModJsonContext.Default.GameBananaFile, ct);
        if (target is null) {
            return null;
        }
        
        await using Stream stream = await GameBanana.Get(fileUrl, ct);
        ITkModParser? parser = _parserManager.GetParser(target.Name);
        
        return parser?.Parse(stream, Unsafe.BitCast<long, Ulid>(fileId), ct) switch {
            { } result => await result,
            _ => null
        };
    }
}