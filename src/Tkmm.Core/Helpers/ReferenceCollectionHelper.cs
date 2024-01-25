using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Tkmm.Core.Generics;

namespace Tkmm.Core.Helpers;

public class ReferenceCollectionHelper
{
    public static void ResolveCollectionChanged(ObservableCollection<Guid> collection, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is IList newItems) {
            foreach (var item in newItems) {
                if (item is IReferenceItem reference && !collection.Contains(reference.Id)) {
                    collection.Add(reference.Id);
                }
            }
        }

        if (e.OldItems is IList oldItems) {
            foreach (var item in oldItems) {
                if (item is IReferenceItem reference) {
                    collection.Remove(reference.Id);
                }
            }
        }
    }
}
