using Mirage.Core.Lifecycle;

namespace Mirage.Core.Utilities.Events;

/// <summary>
/// Represents an active connection between an event and a callback.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to the callback.</typeparam>
public class EventConnection<TPayload>(
    Action<TPayload> callback,
    bool persistent,
    Action disconnect
)
{
    /// <summary>
    /// Gets the callback function executed when the event is dispatched.
    /// </summary>
    public Action<TPayload> Callback { get; } = callback;

    /// <summary>
    /// Gets a value indicating whether the connection persists after being cleared.
    /// </summary>
    public bool Persistent { get; } = persistent;

    /// <summary>
    /// Disconnects the callback from the event.
    /// </summary>
    public Action Disconnect { get; } = disconnect;
}

/// <summary>
/// Provides read-only access to an event.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to event listeners.</typeparam>
public interface IReadOnlyEvent<TPayload> : IReadOnlyDestroyable
{
    /// <summary>
    /// Subscribes a callback function to the event.
    /// </summary>
    /// <param name="callback">The function to be called when the event occurs.</param>
    /// <param name="persistent">
    /// Whether the connection should survive standard clearing operations.
    /// </param>
    /// <returns>An <see cref="EventConnection{TPayload}"/> representing the subscription.</returns>
    EventConnection<TPayload> Connect(Action<TPayload> callback, bool persistent = false);
}

/// <summary>
/// Provides full access to an event, including connection management and destruction.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to event listeners.</typeparam>
public interface IEvent<TPayload> : IReadOnlyEvent<TPayload>, IDestroyable
{
    /// <summary>
    /// Clears event connections. By default, removes only non-persistent connections.
    /// </summary>
    /// <param name="force">
    /// If <see langword="true"/>, clears all connections including persistent ones.
    /// </param>
    void Clear(bool force = false);
}

/// <summary>
/// Base class for managing and dispatching events with type-safe payloads.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to event listeners.</typeparam>
public abstract class Event<TPayload> : Destroyable, IEvent<TPayload>
{
    /// <summary>
    /// Gets the active connections for the event.
    /// </summary>
    protected readonly HashSet<EventConnection<TPayload>> Connections = [];

    /// <summary>
    /// Subscribes a callback function to the event.
    /// </summary>
    /// <param name="callback">The function to be called when the event occurs.</param>
    /// <param name="persistent">
    /// Whether the connection should survive standard clearing operations.
    /// </param>
    /// <returns>An <see cref="EventConnection{TPayload}"/> representing the subscription.</returns>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the event has already been destroyed.
    /// </exception>
    public EventConnection<TPayload> Connect(Action<TPayload> callback, bool persistent = false)
    {
        ThrowIfDestroyed();

        EventConnection<TPayload> connection = null!;

        connection = new(callback, persistent, () => Connections.Remove(connection));

        Connections.Add(connection);

        return connection;
    }

    /// <summary>
    /// Dispatches a payload to all currently connected listeners.
    /// </summary>
    /// <param name="payload">The value passed to each event listener.</param>
    /// <remarks>
    /// A snapshot of the current connections is used so listeners can safely
    /// connect, disconnect, or clear connections while the event is being dispatched.
    /// </remarks>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the event has already been destroyed.
    /// </exception>
    protected void Dispatch(TPayload payload)
    {
        ThrowIfDestroyed();

        foreach (var connection in Connections.ToArray())
            connection.Callback(payload);
    }

    /// <summary>
    /// Clears event connections. By default, removes only non-persistent connections.
    /// </summary>
    /// <param name="force">
    /// If <see langword="true"/>, clears all connections including persistent ones.
    /// </param>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the event has already been destroyed.
    /// </exception>
    public void Clear(bool force = false)
    {
        ThrowIfDestroyed();

        if (force)
        {
            Connections.Clear();
            return;
        }

        Connections.RemoveWhere(connection => !connection.Persistent);
    }

    /// <inheritdoc cref="IDestroyable.Destroy"/>
    protected override void OnDestroy()
    {
        Connections.Clear();
    }
}
