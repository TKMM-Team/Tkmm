namespace Tkmm.Abstractions;

public interface ITkModDependency
{
    /// <summary>
    /// The name of the dependent <see cref="ITkItem"/>.
    /// </summary>
    string DependentName { get; }

    /// <summary>
    /// The ID of the dependent <see cref="ITkModChangelog"/>.
    /// </summary>
    Ulid DependentId { get; }
}