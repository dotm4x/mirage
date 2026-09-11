using Mirage.Core.Exceptions;
using Mirage.Core.Utilities.Events;

namespace Mirage.Core.Utilities.Events;

/// <summary>
/// Exposes a restricted interface for a <see cref="Store{TValue}"/>, including
/// read access through <see cref="Get"/>.
/// </summary>
/// <typeparam name="TValue">The type of the stored value.</typeparam>
public class ReadonlyStore<TValue>(ReadonlyEvent<TValue> @event, Func<TValue> get)
{
    /// <summary>
    /// Gets the current value of the store without allowing mutation.
    /// </summary>
    public Func<TValue> Get { get; } = get;

    /// <summary>
    /// Gets the event interface used to subscribe to changes in the store.
    /// </summary>
    public ReadonlyEvent<TValue> Event { get; } = @event;
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
///     Console.WriteLine($"Value changed to {value}");
/// });
///
/// store.Set(5); // Logs: Value changed to 5
/// </code>
/// </example>
/// <remarks>
/// Creates an instance of a <see cref="Store{TValue}"/>.
/// </remarks>
/// <param name="value">The initial value of the store.</param>
/// <param name="equals">
/// An optional function used to determine whether two values are equal.
/// </param>
public class Store<TValue>(TValue value, Func<TValue, TValue, bool>? equals = null) : Event<TValue>
{
    private TValue value = value;
    private readonly Func<TValue, TValue, bool>? equals = equals;

    /// <summary>
    /// Gets the current value of the store.
    /// </summary>
    public TValue Get()
    {
        return value;
    }

    /// <summary>
    /// Exposes a restricted view of the store, including read-only access
    /// to its current value.
    /// </summary>
    /// <returns>A <see cref="ReadonlyStore{TValue}"/> view of this store.</returns>
    public new ReadonlyStore<TValue> AsReadonly()
    {
        return new(base.AsReadonly(), Get);
    }

    /// <summary>
    /// Updates the store's value and notifies listeners if the value has changed.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the store has already been destroyed.
    /// </exception>
    public void Set(TValue value)
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException("Store is destroyed, cannot set value");
        }

        if (equals is not null)
        {
            if (equals(this.value, value))
                return;
        }
        else if (EqualityComparer<TValue>.Default.Equals(this.value, value))
        {
            return;
        }

        this.value = value;

        foreach (var connection in Connections)
        {
            try
            {
                connection.Callback(this.value);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Error in store listener: {exception}");

                throw;
            }
        }
    }
}
