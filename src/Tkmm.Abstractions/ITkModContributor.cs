namespace Tkmm.Abstractions;

public interface ITkModContributor
{
    /// <summary>
    /// The author of this contribution.
    /// </summary>
    string Author { get; set; }

    /// <summary>
    /// A description of the contribution.
    /// </summary>
    string Contribution { get; set; }
}