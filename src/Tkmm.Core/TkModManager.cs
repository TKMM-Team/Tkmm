using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.IO;
using Tkmm.Core.Abstractions.Parsers;

namespace Tkmm.Core;

internal sealed partial class TkModManager(ITkFileSystem fs, ITkModParserManager parserManager)
    : ObservableObject, ITkModManager
{
    private const string PROFILES_FILE = "profiles.json";

    private readonly ITkFileSystem _fs = fs;
    private readonly ITkModParserManager _parserManager = parserManager;
    private ObservableCollection<ITkProfile> _profiles = [];
    private ObservableCollection<ITkMod> _mods = [];

    public IList<ITkMod> Mods => _mods;

    [ObservableProperty]
    private ITkProfile _currentProfile = null!;

    public IList<ITkProfile> Profiles => _profiles;

    public async ValueTask<ITkMod?> Create(string argument, Stream? stream, Ulid id, CancellationToken ct)
    {
        if (_parserManager.GetParser(argument) is not ITkModParser parser) {
            throw Exceptions.ParserNotFound(argument);
        }

        return stream switch {
            not null => await parser.Parse(stream, id, ct),
            _ => await parser.Parse(argument, id, ct)
        };
    }

    public ValueTask Merge(ITkProfile profile, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask Initialize(CancellationToken ct)
    {
        ProfilesMetadata? profilesMetadata = await _fs.GetMetadata(
            PROFILES_FILE, ProfilesMetadataSerializerContext.Default.ProfilesMetadata);

        _mods = await _fs.GetMetadata<ObservableCollection<ITkMod>>("mods")
                ?? [];

        _profiles = profilesMetadata is not null
            ? [..profilesMetadata.Profiles]
            : [];

        CurrentProfile = profilesMetadata?.Profiles
                             .FirstOrDefault(x => x.Id == profilesMetadata.CurrentProfileId)
                         ?? new TkProfile();
    }

    public ValueTask SaveState()
    {
        throw new NotImplementedException();
    }

    private record ProfilesMetadata(List<TkProfile> Profiles, Ulid CurrentProfileId);

    [JsonSerializable(typeof(ProfilesMetadata))]
    private partial class ProfilesMetadataSerializerContext : JsonSerializerContext;
}