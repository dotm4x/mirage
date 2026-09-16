using Mirage.Core.Lifecycle;

namespace Mirage.Core.Events;

/// <summary>
/// Provides read-only access to a store.
/// </summary>
/// <typeparam name="TValue">The type of the stored value.</typeparam>
public interface IReadOnlyStore<TValue> : IReadOnlyEvent<TValue>
{
    /// <summary>
    /// Gets the current value of the store.
    /// </summary>
    TValue Get();

    /// <summary>
    /// Gets the value held by the store before the most recent change.
    /// </summary>
    TValue Previous { get; }
}

/// <summary>
/// Provides full access to a store, including value mutation.
/// </summary>
/// <typeparam name="TValue">The type of the stored value.</typeparam>
public interface IStore<TValue> : IReadOnlyStore<TValue>, IEvent<TValue>
{
    /// <summary>
    /// Updates the store's value and notifies listeners if the value has changed.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    void Set(TValue value);
}

/// <summary>
/// Represents a reactive state container that holds a value and notifies listeners
/// when the value changes.
/// </summary>
/// <typeparam name="TValue">The type of value stored in the container.</typeparam>
/// <example>
/// <code>
/// var store = new Store&lt;int&gt;(0);
///
/// store.Connect(value =>
/// {
///     Console.WriteLine($"Changed from {store.Previous} to {value}.");
/// });
///
/// store.Set(5); // Logs: Changed from 0 to 5.
/// </code>
/// </example>
/// <remarks>
/// Initializes a new instance of the <see cref="Store{TValue}"/> class.
/// </remarks>
/// <param name="value">The initial value of the store.</param>
/// <param name="equals">
/// An optional function used to determine whether two values are equal.
/// </param>
public class Store<TValue>(TValue value, Func<TValue, TValue, bool>? equals = null)
    : Event<TValue>,
        IStore<TValue>
{
    private readonly Func<TValue, TValue, bool>? _equals = equals;
    private TValue _value = value;

    /// <summary>
    /// Gets the current value of the store.
    /// </summary>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the store has already been destroyed.
    /// </exception>
    public TValue Get()
    {
        return _value;
    }

    /// <summary>
    /// Gets the value held by the store before the most recent change.
    /// </summary>
    public TValue Previous { get; private set; } = value;

    /// <summary>
    /// Updates the store's value and notifies listeners if the value has changed.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the store has already been destroyed.
    /// </exception>
    public void Set(TValue value)
    {
        ThrowIfDestroyed();

        if (_equals is not null)
        {
            if (_equals(_value, value))
                return;
        }
        else if (EqualityComparer<TValue>.Default.Equals(_value, value))
        {
            return;
        }

        Previous = _value;
        _value = value;

        Dispatch(_value);
    }
}