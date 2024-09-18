using Tkmm.Core.Abstractions.Common;

namespace Tkmm.Core.Abstractions;

public interface ITkItem
{
    Ulid Id { get; }

    string Name { get; set; }

    string Description { get; set; }

    IThumbnail? Thumbnail { get; set; }
}