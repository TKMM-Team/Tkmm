namespace Tkmm.Core.Abstractions;

public interface ITkProfile : ITkItem
{
    Ulid Id { get; }
    
    /// <summary>
    /// The <see cref="ITkMod"/> references in this <see cref="ITkProfile"/>.
    /// </summary>
    IList<ITkProfileMod> Mods { get; }
    
    /// <summary>
    /// The current selected <see cref="ITkProfileMod"/>.
    /// </summary>
    ITkProfileMod? Selected { get; set; }
    
    /// <summary>
    /// Moves the <paramref name="target"/> up in the <see cref="Mods"/> collection.
    /// </summary>
    /// <param name="target">The target <see cref="ITkMod"/> to be repositioned.</param>
    ITkProfileMod MoveUp(ITkProfileMod target) => Move(target, direction: -1);

    /// <summary>
    /// Moves the <paramref name="target"/> down in the <see cref="Mods"/> collection.
    /// </summary>
    /// <param name="target">The target <see cref="ITkMod"/> to be repositioned.</param>
    ITkProfileMod MoveDown(ITkProfileMod target) => Move(target, direction: 1);

    /// <summary>
    /// Move the <paramref name="target"/> in the provided <paramref name="direction"/>.<br/>
    /// <i><b>Note:</b> <c>0</c> is the highest position.</i>
    /// </summary>
    /// <param name="target">The target <see cref="ITkMod"/> to be repositioned.</param>
    /// <param name="direction">The direction to move.</param>
    protected ITkProfileMod Move(ITkProfileMod target, int direction)
    {
        int currentIndex = Mods.IndexOf(target);
        int newIndex = currentIndex + direction;

        if (newIndex < 0 || newIndex >= Mods.Count) {
            return target;
        }

        ITkProfileMod store = Mods[newIndex];
        Mods[newIndex] = target;
        Mods[currentIndex] = store;
        
        return target;
    }
}