namespace Tkmm.GameBanana.Core;

public enum GameBananaSortMode
{
    Default,
    New,
    Updated
}

public interface IGameBananaSource
{
    public static GameBananaSortMode[] GameBananaSortModes => Enum.GetValues<GameBananaSortMode>();
    
    /// <summary>
    /// The 0-based page index.
    /// </summary>
    int CurrentPage { get; set; }
    
    /// <summary>
    /// The active sorting mode.
    /// </summary>
    GameBananaSortMode SortMode { get; set; }
    
    /// <summary>
    /// The active sorting mode.
    /// </summary>
    GameBananaFeed? Feed { get; }

    /// <summary>
    /// Search for a keyword and update the <see cref="Feed"/>.
    /// </summary>
    /// <param name="searchTerm">The term to search for (must be longer than 2 characters)</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask Search(string searchTerm, CancellationToken ct = default)
        => LoadPage(CurrentPage = 0, searchTerm, customFeed: null, ct);

    /// <summary>
    /// Reload the <see cref="CurrentPage"/>.
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask Reload(CancellationToken ct = default)
        => LoadPage(CurrentPage, ct: ct);

    /// <summary>
    /// Load the next page.
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <param name="customFeed">A custom feed to use in place of fetching the GameBanana API.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask LoadNextPage(string? searchTerm = null, GameBananaFeed? customFeed = null, CancellationToken ct = default)
        => LoadPage(++CurrentPage, null, customFeed, ct);

    /// <summary>
    /// Load the specified <paramref name="page"/>.
    /// </summary>
    /// <param name="page">The 0-based page index.</param>
    /// <param name="searchTerm"></param>
    /// <param name="customFeed">A custom feed to use in place of fetching the GameBanana API.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask LoadPage(int page, string? searchTerm = null, GameBananaFeed? customFeed = null, CancellationToken ct = default);
}