namespace Tkmm.Core.Abstractions;

public interface ITkProfile : ITkItem
{
    /// <summary>
    /// The <see cref="ITkMod"/> references in this <see cref="ITkProfile"/>.
    /// </summary>
    IList<ITkProfileMod> Mods { get; }
    
    /// <summary>
    /// Moves the <paramref name="target"/> up in the <see cref="Mods"/> collection.
    /// </summary>
    /// <param name="target">The target <see cref="ITkMod"/> to be repositioned.</param>
    void MoveUp(ITkMod target) => Move(target, direction: -1);

    /// <summary>
    /// Moves the <paramref name="target"/> down in the <see cref="Mods"/> collection.
    /// </summary>
    /// <param name="target">The target <see cref="ITkMod"/> to be repositioned.</param>
    void MoveDown(ITkMod target) => Move(target, direction: 1);

    /// <summary>
    /// Move the <paramref name="target"/> in the provided <paramref name="direction"/>.<br/>
    /// <i><b>Note:</b> <c>0</c> is the highest position.</i>
    /// </summary>
    /// <param name="target">The target <see cref="ITkMod"/> to be repositioned.</param>
    /// <param name="direction">The direction to move.</param>
    protected void Move(ITkMod target, int direction);
}