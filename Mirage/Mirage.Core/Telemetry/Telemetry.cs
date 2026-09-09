using Mirage.Core.Interfaces;
using Mirage.Core.Telemetry.Ports;
using Mirage.Core.Utilities.Collections;
using Mirage.Core.Utilities.Events;

namespace Mirage.Core.Telemetry;

/// <summary>
///   Represents a telemetry manager responsible for collecting, organizing,
///   and dispatching messages across multiple prioritized output ports.
/// </summary>
public class Telemetry : IDestroyable
{
    /// <summary>
    ///   The group containing all registered telemetry output ports.
    /// </summary>
    public readonly Group<IPort> Ports = [];

    public bool Destroyed { get; private set; }

    private readonly Signal<Message> onSend = new();

    /// <summary>
    ///   Event fired when a message has been sent.
    /// </summary>
    public readonly ReadonlyEvent<Message> OnSend;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Telemetry"/> class.
    /// </summary>
    /// <param name="ports">
    ///   The initial telemetry output ports to register.
    /// </param>
    public Telemetry(IEnumerable<IPort>? ports = null)
    {
        foreach (var port in ports ?? [])
        {
            Ports.Add(port);
        }

        OnSend = onSend.AsReadonly();
    }

    /// <summary>
    ///   Dispatches a message to all registered output ports in descending
    ///   order of their priority.
    /// </summary>
    /// <param name="message">
    ///   The message to dispatch.
    /// </param>
    /// <returns>
    ///   The dispatched message.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///   Thrown when telemetry has already been destroyed.
    /// </exception>
    public Message Send(Message message)
    {
        if (Destroyed)
        {
            throw new InvalidOperationException("Telemetry is destroyed, cannot send messages");
        }

        IPort[] sortedPorts = [.. Ports.OrderByDescending(port => port.Priority)];

        foreach (var port in sortedPorts)
        {
            port.Send(message);
        }

        onSend.Fire(message);

        return message;
    }

    public void Destroy()
    {
        if (Destroyed)
        {
            throw new InvalidOperationException(
                "Telemetry is already destroyed, cannot destroy again"
            );
        }

        foreach (var port in Ports)
        {
            port.Destroy();
        }

        Ports.Destroy();

        Destroyed = true;
    }
}
