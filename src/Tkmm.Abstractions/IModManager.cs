namespace Tkmm.Abstractions;

public interface IModManager : IModStorage
{
    /// <summary>
    /// Initialize the mod manager.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    ValueTask Initialize(CancellationToken ct = default);
    
    /// <summary>
    /// Persist the current state of the mod manager.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    ValueTask Save(CancellationToken ct = default);

    /// <summary>
    /// Merge the current selected profile.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    ValueTask Merge(CancellationToken ct = default) => Merge(CurrentProfile, ct);
    
    /// <summary>
    /// Merge the provided <paramref name="profile"/>.
    /// </summary>
    /// <param name="profile">The profile to merge.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    ValueTask Merge(ITkProfile profile, CancellationToken ct = default);

    /// <summary>
    /// Attempts to install the provided <paramref name="input"/> into the mod manager using the
    /// provided <paramref name="id"/> when applicable. 
    /// </summary>
    /// <param name="input">The object to import.</param>
    /// <param name="id">The predefined ID of the mod.</param>
    /// <param name="stream">An optional stream to provide the input data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The installed mod if the operation was successful, otherwise null.</returns>
    ValueTask<ITkMod?> Install(object? input, Stream? stream = null, Ulid id = default, CancellationToken ct = default);
    
    /// <summary>
    /// Gets the contents of the specified file and returns a stream to the data.
    /// </summary>
    /// <param name="targetMod">The mod to read data from.</param>
    /// <param name="manifestFileName">The manifest name of the file to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<Stream> GetModFile(ITkMod targetMod, string manifestFileName, CancellationToken ct = default);

    /// <summary>
    /// Creates a new mod file and writes the provided <paramref name="data"/> into it.
    /// </summary>
    /// <param name="targetMod">The mod to write data into.</param>
    /// <param name="manifestFileName">The manifest name of the file to write.</param>
    /// <param name="data">The contents of the new file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    ValueTask CreateModFile(ITkMod targetMod, string manifestFileName, Span<byte> data, CancellationToken ct = default);
}