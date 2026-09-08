using Mirage.Core.Interfaces;

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
/// Exposes a restricted interface for subscribing to events without allowing
/// dispatching or clearing.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to event listeners.</typeparam>
public class ReadonlyEvent<TPayload>(Action<Action<TPayload>> connect)
{
    /// <summary>
    /// Gets the function used to subscribe a callback to the event.
    /// </summary>
    public Action<Action<TPayload>> Connect { get; } = connect;
}

/// <summary>
/// Base class for managing and dispatching events with type-safe payloads.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to event listeners.</typeparam>
public abstract class Event<TPayload> : IDestroyable
{
    /// <summary>
    /// Internal set of active event connections.
    /// </summary>
    protected HashSet<EventConnection<TPayload>> Connections = [];

    public bool Destroyed { get; private set; }

    /// <summary>
    /// Subscribes a callback function to the event.
    /// </summary>
    /// <param name="callback">The function to be called when the event occurs.</param>
    /// <param name="persistent">
    /// Whether the connection should survive standard clearing operations.
    /// Defaults to <see langword="false"/>.
    /// </param>
    /// <returns>An <see cref="EventConnection{TPayload}"/> representing the subscription.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the event has already been destroyed.
    /// </exception>
    public EventConnection<TPayload> Connect(Action<TPayload> callback, bool persistent = false)
    {
        if (Destroyed)
        {
            throw new InvalidOperationException("Event is destroyed, cannot connect");
        }

        EventConnection<TPayload> connection = null!;

        connection = new(callback, persistent, () => Connections.Remove(connection));

        Connections.Add(connection);

        return connection;
    }

    /// <summary>
    /// Clears event connections. By default, removes only non-persistent connections.
    /// </summary>
    /// <param name="force">
    /// If <see langword="true"/>, clears all connections including persistent ones.
    /// Defaults to <see langword="false"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the event has already been destroyed.
    /// </exception>
    public void Clear(bool force = false)
    {
        if (Destroyed)
        {
            throw new InvalidOperationException("Event is destroyed, cannot clear");
        }

        if (force)
        {
            Connections.Clear();
        }
        else
        {
            Connections.RemoveWhere(connection => !connection.Persistent);
        }
    }

    /// <summary>
    /// Exposes a restricted interface of the event.
    /// </summary>
    /// <returns>
    /// A <see cref="ReadonlyEvent{TPayload}"/> that allows subscribing to the event
    /// without providing access to its internal operations.
    /// </returns>
    public ReadonlyEvent<TPayload> AsReadonly()
    {
        return new(callback => Connect(callback));
    }

    public void Destroy()
    {
        if (Destroyed)
        {
            throw new InvalidOperationException("Event is already destroyed, cannot destroy again");
        }

        Connections.Clear();
        Destroyed = true;
    }
}
