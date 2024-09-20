using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.Common;

namespace Tkmm.Core;

public sealed class TkProfile : ITkProfile
{
    public Ulid Id { get; } = Ulid.NewUlid();

    public string Name { get; set; } = SystemMsg.DefaultProfileName;

    public string Description { get; set; } = string.Empty;

    public IThumbnail? Thumbnail { get; set; }

    public IList<ITkProfileMod> Mods { get; } = [];
}