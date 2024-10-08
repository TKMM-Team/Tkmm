namespace Tkmm.Abstractions;

public interface ITkItem
{
    /// <summary>
    /// The name of this item.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// The description of this item.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// The thumbnail of this item.
    /// </summary>
    ITkThumbnail? Thumbnail { get; set; }
}