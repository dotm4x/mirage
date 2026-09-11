using System.Collections;
using Mirage.Core.Exceptions;
using Mirage.Core.Interfaces;
using Mirage.Core.Utilities.Events;

namespace Mirage.Core.Utilities.Collections;

/// <summary>
/// Provides read-only access to the items in a group.
/// </summary>
/// <typeparam name="TItem">The type of items stored in the group.</typeparam>
public class ReadonlyGroup<TItem>(IEnumerable<TItem> items) : IEnumerable<TItem>
{
    #region Properties

    /// <summary>
    /// Gets the number of items currently contained in the group.
    /// </summary>
    public int Count { get; } = items.Count();

    #endregion

    #region Collection Methods

    /// <summary>
    /// Determines whether the specified item is contained in the group.
    /// </summary>
    /// <param name="item">The item to locate.</param>
    /// <returns>
    /// <see langword="true"/> if the item is contained in the group; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(TItem item)
    {
        return items.Contains(item);
    }

    /// <summary>
    /// Performs the specified action on each item in the group.
    /// </summary>
    /// <param name="action">The action to perform for each item.</param>
    public void ForEach(Action<TItem> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    /// <summary>
    /// Creates an array containing all items in the group.
    /// </summary>
    /// <returns>A new array containing the items in the group.</returns>
    public TItem[] ToArray()
    {
        return [.. items];
    }

    /// <summary>
    /// Creates a list containing all items in the group.
    /// </summary>
    /// <returns>A new list containing the items in the group.</returns>
    public List<TItem> ToList()
    {
        return [.. items];
    }

    /// <summary>
    /// Returns an enumerator that iterates through the items in the group.
    /// </summary>
    /// <returns>An enumerator for the group.</returns>
    public IEnumerator<TItem> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the items in the group.
    /// </summary>
    /// <returns>An enumerator for the group.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}

/// <summary>
/// Represents a mutable collection of unique items, providing reactive events
/// and an optional capacity limit with automatic truncation.
/// </summary>
/// <typeparam name="TItem">The type of items stored in the group.</typeparam>
public class Group<TItem> : IDestroyable, IEnumerable<TItem>
{
    private readonly List<TItem> items = [];
    private readonly Signal<TItem> onAdd = new();
    private readonly Signal<TItem> onRemove = new();
    private readonly Signal<Unit> onClear = new();

    #region Properties and Events

    /// <summary>
    /// Gets the maximum number of items allowed in the group. A value of
    /// <c>0</c> indicates unlimited capacity.
    /// </summary>
    public readonly int Limit;

    /// <summary>
    /// Gets the number of items currently contained in the group.
    /// </summary>
    public int Count => items.Count;

    /// <inheritdoc cref="IDestroyable.Destroyed"/>
    public bool Destroyed { get; private set; }

    /// <summary>
    /// Gets the event fired whenever an item is added to the group.
    /// </summary>
    public ReadonlyEvent<TItem> OnAdd { get; }

    /// <summary>
    /// Gets the event fired whenever an item is removed from the group.
    /// </summary>
    public ReadonlyEvent<TItem> OnRemove { get; }

    /// <summary>
    /// Gets the event fired when the group is cleared.
    /// </summary>
    public ReadonlyEvent<Unit> OnClear { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Group{TItem}"/> class.
    /// </summary>
    /// <param name="items">
    /// The initial items to add to the group, or <see langword="null"/> to
    /// start empty.
    /// </param>
    /// <param name="limit">
    /// The maximum number of items allowed in the group.
    /// A value of <c>0</c> means unlimited capacity.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="limit"/> is negative.
    /// </exception>
    public Group(IEnumerable<TItem>? items = null, int limit = 0)
    {
        if (limit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit cannot be negative");
        }

        Limit = limit;

        foreach (var item in items ?? [])
        {
            Add(item);
        }

        OnAdd = onAdd.AsReadonly();
        OnRemove = onRemove.AsReadonly();
        OnClear = onClear.AsReadonly();
    }

    #endregion

    #region Private Methods

    private void ThrowIfDestroyed()
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException("Group is destroyed");
        }
    }

    private void Truncate()
    {
        if (Limit > 0 && items.Count >= Limit)
        {
            Remove(items[0]);
        }
    }

    #endregion

    #region Collection Methods

    /// <summary>
    /// Adds one or more items to the group.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <returns>The items that were added.</returns>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the group has been destroyed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when an item is already contained in the group.
    /// </exception>
    public TItem[] Add(params TItem[] items)
    {
        ThrowIfDestroyed();

        foreach (var item in items)
        {
            if (this.items.Contains(item))
            {
                throw new InvalidOperationException(
                    "Item is already in the group, cannot add again"
                );
            }

            Truncate();
            this.items.Add(item);
            onAdd.Fire(item);
        }

        return items;
    }

    /// <summary>
    /// Removes one or more items from the group.
    /// </summary>
    /// <param name="items">The items to remove.</param>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the group has been destroyed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when an item is not contained in the group.
    /// </exception>
    public void Remove(params TItem[] items)
    {
        ThrowIfDestroyed();

        foreach (var item in items)
        {
            if (!this.items.Contains(item))
            {
                throw new InvalidOperationException("Item is not in the group, cannot remove");
            }

            onRemove.Fire(item);
            this.items.Remove(item);
        }
    }

    /// <summary>
    /// Determines whether the specified item is contained in the group.
    /// </summary>
    /// <param name="item">The item to locate.</param>
    /// <returns>
    /// <see langword="true"/> if the item is contained in the group; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(TItem item)
    {
        return items.Contains(item);
    }

    /// <summary>
    /// Performs the specified action on each item in the group.
    /// </summary>
    /// <param name="action">The action to perform for each item.</param>
    public void ForEach(Action<TItem> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    /// <summary>
    /// Creates an array containing all items in the group.
    /// </summary>
    /// <returns>A new array containing the items in the group.</returns>
    public TItem[] ToArray()
    {
        return [.. items];
    }

    /// <summary>
    /// Creates a list containing all items in the group.
    /// </summary>
    /// <returns>A new list containing the items in the group.</returns>
    public List<TItem> ToList()
    {
        return [.. items];
    }

    /// <summary>
    /// Returns an enumerator that iterates through the items in the group.
    /// </summary>
    /// <returns>An enumerator for the group.</returns>
    public IEnumerator<TItem> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the items in the group.
    /// </summary>
    /// <returns>An enumerator for the group.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Removes all items from the group.
    /// </summary>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the group has been destroyed.
    /// </exception>
    public void Clear()
    {
        ThrowIfDestroyed();

        onClear.Fire(Unit.Value);

        foreach (var item in items.ToArray())
        {
            Remove(item);
        }
    }

    /// <summary>
    /// Creates a read-only view of the group without access to its mutating
    /// operations.
    /// </summary>
    /// <returns>
    /// A <see cref="ReadonlyGroup{TItem}"/> that allows reading the group's items
    /// without providing access to its mutating operations.
    /// </returns>
    public ReadonlyGroup<TItem> AsReadonly()
    {
        return new ReadonlyGroup<TItem>(items);
    }

    #endregion

    #region Lifecycle Methods

    /// <inheritdoc cref="IDestroyable.Destroy"/>
    public void Destroy()
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException("Group is already destroyed, cannot destroy again");
        }

        Clear();

        onAdd.Destroy();
        onRemove.Destroy();
        onClear.Destroy();

        Destroyed = true;
    }

    #endregion
}
