using System.Collections.ObjectModel;
using Tkmm.Core.Generics;

namespace Tkmm.Core.Helpers;

public static class CollectionHelper
{
    public static bool TryInsert<T>(this ObservableCollection<T> items, T item) where T : IReferenceItem
    {
        if (items.FirstOrDefault(x => x.Id == item.Id) is T match && items.IndexOf(match) is int index && index > -1) {
            items[index] = item;
            return true;
        }

        items.Add(item);
        return false;
    }
}
