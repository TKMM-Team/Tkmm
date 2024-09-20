using System.Text.Json.Serialization;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.IO;

namespace Tkmm.Core;

public sealed partial class TkModManager(ITkFileSystem fs) : ITkModManager
{
    private const string PROFILES_FILE = "profiles.json";
    
    private readonly ITkFileSystem _fs = fs;

    public IList<ITkMod> Mods { get; private set; } = null!;

    public ITkProfile CurrentProfile { get; set; } = null!;

    public IList<ITkProfile> Profiles { get; private set; } = null!;

    public ValueTask Merge(ITkProfile profile, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask InitializeAsync()
    {
        ProfilesMetadata? profilesMetadata = await _fs.GetMetadata(
            PROFILES_FILE, ProfilesMetadataSerializerContext.Default.ProfilesMetadata);
        
        Mods = await _fs.GetMetadata<IList<ITkMod>>("mods")
               ?? [];
        
        Profiles = profilesMetadata is not null
            ? [..profilesMetadata.Profiles] : [];
        
        CurrentProfile = profilesMetadata?.Profiles
            .FirstOrDefault(x => x.Id == profilesMetadata.CurrentProfileId)
                ?? new TkProfile();
    }

    private record ProfilesMetadata(List<TkProfile> Profiles, Ulid CurrentProfileId);

    [JsonSerializable(typeof(ProfilesMetadata))]
    private partial class ProfilesMetadataSerializerContext : JsonSerializerContext;
}
