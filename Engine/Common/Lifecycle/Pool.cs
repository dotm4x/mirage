namespace Mirage.Common.Lifecycle;

/// <summary>
/// Defines an object that can be acquired and released by a pool.
/// </summary>
public interface IPoolable : IRestorable
{
    /// <summary>
    /// Called when the object is acquired from a pool.
    /// </summary>
    void OnAcquire();

    /// <summary>
    /// Called when the object is released back to a pool.
    /// </summary>
    void OnRelease();
}

/// <summary>
/// Represents a pool of reusable objects.
/// </summary>
/// <typeparam name="TItem">
/// The type of objects managed by the pool.
/// </typeparam>
/// <param name="factory">
/// The factory used to create objects when no available objects exist.
/// </param>
public class Pool<TItem>(Func<TItem> factory)
    where TItem : IPoolable
{
    private readonly Stack<TItem> _available = [];

    /// <summary>
    /// Acquires an object from the pool.
    /// </summary>
    /// <returns>
    /// An available object from the pool, or a newly created object when
    /// no available objects exist.
    /// </returns>
    public TItem Acquire()
    {
        var item = _available.Count > 0 ? _available.Pop() : factory();

        item.OnAcquire();

        return item;
    }

    /// <summary>
    /// Releases an object back to the pool.
    /// </summary>
    /// <param name="item">
    /// The object to release.
    /// </param>
    public void Release(TItem item)
    {
        item.OnRelease();
        item.Restore();

        _available.Push(item);
    }
}
