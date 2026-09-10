using System.Collections;
using Mirage.Core.Interfaces;
using Mirage.Core.Utilities.Events;

namespace Mirage.Core.Utilities.Collections;

/// <summary>
/// Exposes a restricted interface for reading the items in a group without
/// allowing modification or destruction.
/// </summary>
/// <typeparam name="TItem">The type of items stored in the group.</typeparam>
public class ReadonlyGroup<TItem>(IEnumerable<TItem> items) : IEnumerable<TItem>
{
    /// <summary>
    /// Gets the number of items currently contained in the group.
    /// </summary>
    public int Count { get; } = items.Count();

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
}

/// <summary>
/// Represents a mutable collection of unique items, providing reactive events
/// and an optional capacity limit with automatic truncation.
/// </summary>
/// <typeparam name="TItem">The type of items stored in the group.</typeparam>
public class Group<TItem> : IDestroyable, IEnumerable<TItem>
{
    /// <summary>
    /// The maximum number of items allowed in the group.
    /// A value of <c>0</c> means the group has no limit.
    /// </summary>
    public readonly int Limit;

    private readonly List<TItem> items = [];

    /// <summary>
    /// Gets the number of items currently contained in the group.
    /// </summary>
    public int Count => items.Count;

    /// <summary>
    /// Gets a value indicating whether the group has been destroyed.
    /// </summary>
    public bool Destroyed { get; private set; }

    private readonly Signal<TItem> onAdd = new();

    /// <summary>
    /// Event fired whenever an item is added to the group.
    /// </summary>
    public ReadonlyEvent<TItem> OnAdd { get; }

    private readonly Signal<TItem> onRemove = new();

    /// <summary>
    /// Event fired whenever an item is removed from the group.
    /// </summary>
    public ReadonlyEvent<TItem> OnRemove { get; }

    private readonly Signal<Unit> onClear = new();

    /// <summary>
    /// Event fired when the group is cleared.
    /// </summary>
    public ReadonlyEvent<Unit> OnClear { get; }

    /// <summary>
    /// Creates a new instance of a <see cref="Group{TItem}"/>.
    /// </summary>
    /// <param name="items">
    /// The initial items to add to the group, or <see langword="null"/> to start empty.
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

    private void ThrowIfDestroyed()
    {
        if (Destroyed)
        {
            throw new InvalidOperationException("Group is destroyed");
        }
    }

    private void Truncate()
    {
        if (Limit > 0 && items.Count >= Limit)
        {
            Remove(items[0]);
        }
    }

    /// <summary>
    /// Adds one or more items to the group.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <returns>The items that were added.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the group has been destroyed or an item is already contained in the group.
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
    /// <exception cref="InvalidOperationException">
    /// Thrown when the group has been destroyed or an item is not contained in the group.
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
    /// <exception cref="InvalidOperationException">
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
    /// Exposes a restricted interface of the group without allowing modification
    /// or destruction.
    /// </summary>
    /// <returns>
    /// A <see cref="ReadonlyGroup{TItem}"/> that allows reading the group's items
    /// without providing access to its mutating operations.
    /// </returns>
    public ReadonlyGroup<TItem> AsReadonly()
    {
        return new ReadonlyGroup<TItem>(items);
    }

    public void Destroy()
    {
        if (Destroyed)
        {
            throw new InvalidOperationException("Group is already destroyed, cannot destroy again");
        }

        Clear();

        onAdd.Destroy();
        onRemove.Destroy();
        onClear.Destroy();

        Destroyed = true;
    }
}
