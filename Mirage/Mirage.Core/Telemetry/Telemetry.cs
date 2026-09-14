using Mirage.Core.Lifecycle;
using Mirage.Core.Telemetry.Ports;
using Mirage.Core.Utilities.Collections;
using Mirage.Core.Utilities.Events;

namespace Mirage.Core.Telemetry;

/// <summary>
/// Represents a telemetry manager responsible for collecting, organizing,
/// and dispatching messages across multiple prioritized output ports.
/// </summary>
public sealed class Telemetry : Destroyable
{
    private readonly Signal<Message> _onSend = new();

    /// <summary>
    /// Gets the registered telemetry output ports.
    /// </summary>
    public readonly Group<IPort> Ports = [];

    /// <summary>
    /// Gets the signal fired after a message is dispatched to all output ports.
    /// </summary>
    public IReadOnlyEvent<Message> OnSend { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Telemetry"/> class.
    /// </summary>
    /// <param name="ports">
    /// The initial telemetry output ports to register.
    /// </param>
    public Telemetry(IEnumerable<IPort>? ports = null)
    {
        foreach (var port in ports ?? [])
            Ports.Add(port);

        OnSend = _onSend;
    }

    /// <summary>
    /// Dispatches a telemetry message using the specified content and optional
    /// message information.
    /// </summary>
    /// <param name="content">
    /// The content of the message.
    /// </param>
    /// <param name="source">
    /// The optional origin component, service, or module of the message.
    /// </param>
    /// <param name="kind">
    /// The severity or classification of the message.
    /// </param>
    /// <param name="metadata">
    /// Optional contextual key-value metadata associated with the message.
    /// </param>
    /// <returns>
    /// The dispatched message.
    /// </returns>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when telemetry has already been destroyed.
    /// </exception>
    public Message Send(
        string content,
        string source = "",
        MessageKind kind = MessageKind.Information,
        IReadOnlyDictionary<string, object?>? metadata = null
    )
    {
        return Send(new Message(content, source, kind, metadata));
    }

    /// <summary>
    /// Dispatches an existing telemetry message to all registered output ports
    /// in descending order of their priority.
    /// </summary>
    /// <param name="message">
    /// The message to dispatch.
    /// </param>
    /// <returns>
    /// The dispatched message.
    /// </returns>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when telemetry has already been destroyed.
    /// </exception>
    public Message Send(Message message)
    {
        ThrowIfDestroyed();

        IPort[] sortedPorts = [.. Ports.OrderByDescending(port => port.Priority)];

        foreach (var port in sortedPorts)
            port.Send(message);

        _onSend.Fire(message);

        return message;
    }

    /// <inheritdoc cref="Destroyable.Destroy"/>
    protected override void OnDestroy()
    {
        foreach (var port in Ports)
            port.Destroy();

        Ports.Destroy();
        _onSend.Destroy();
    }
}
