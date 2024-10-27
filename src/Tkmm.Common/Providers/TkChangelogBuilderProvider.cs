using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Abstractions.Services;
using Tkmm.Common.ChangelogBuilders;
using Tkmm.Common.ChangelogBuilders.GameData;
using Tkmm.Common.ChangelogBuilders.Mals;
using Tkmm.Common.ChangelogBuilders.Rsdb;

namespace Tkmm.Common.Providers;

public sealed class TkChangelogBuilderProvider : IChangelogBuilderProvider
{
    private readonly GameDataChangelogBuilder _gameDataChangelogBuilder = new();
    private readonly RsdbChangelogBuilder _rsdbChangelogBuilder = new();
    private readonly TagDbChangelogBuilder _tagDbChangelogBuilder = new();
    private readonly MalsChangelogBuilder _malsChangelogBuilder;
    private readonly SarcChangelogBuilder _sarcChangelogBuilder;

    public TkChangelogBuilderProvider(IRomfs romfs)
    {
        _malsChangelogBuilder = new MalsChangelogBuilder(romfs);
        _sarcChangelogBuilder = new SarcChangelogBuilder(romfs, this);
    }
    
    public IChangelogBuilder? GetChangelogBuilder(in TkFileInfo fileInfo)
    {
        if (fileInfo.Canonical.Length < 4) {
            throw new ArgumentException($"Invalid canonical file path: '{fileInfo.Canonical}'",
                nameof(fileInfo));
        }
        
        return fileInfo switch {
            { Canonical: "GameData/GameDataList.Product.byml" } => _gameDataChangelogBuilder,
            { Canonical: "RSDB/Tag.Product.rstbl.byml" } => _tagDbChangelogBuilder,
            { } when fileInfo.Canonical[..4] is "RSDB" => _rsdbChangelogBuilder,
            { Extension: ".sarc" } when fileInfo.Canonical[..4] is "Mals" => _malsChangelogBuilder,
            { Extension: ".bfarc" or ".bkres" or ".blarc" or ".genvb" or ".pack" or ".ta" } => _sarcChangelogBuilder,
            { Extension: ".bgyml" } => BymlChangelogBuilder.Instance,
            { Extension: ".byml" } when fileInfo.Canonical[..4] is not "RSDB" && fileInfo.Canonical[..8] is not "GameData" => BymlChangelogBuilder.Instance,
            _ => null
        };
    }
}