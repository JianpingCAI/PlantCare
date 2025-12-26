using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PlantCare.App.Utils;

/// <summary>
/// Observable collection with bulk operations support
/// </summary>
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    private bool _suppressNotification = false;

    public ObservableRangeCollection() : base()
    {
    }

    public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    /// <summary>
    /// Adds a range of items to the collection
    /// </summary>
    public void AddRange(IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        _suppressNotification = true;

        foreach (T? item in items)
        {
            Add(item);
        }

        _suppressNotification = false;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Replaces all items in the collection with the new items
    /// </summary>
    public void ReplaceRange(IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        _suppressNotification = true;

        Clear();
        foreach (T? item in items)
        {
            Add(item);
        }

        _suppressNotification = false;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Removes a range of items from the collection
    /// </summary>
    public void RemoveRange(IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        _suppressNotification = true;

        foreach (T? item in items)
        {
            Remove(item);
        }

        _suppressNotification = false;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppressNotification)
        {
            base.OnCollectionChanged(e);
        }
    }
}
