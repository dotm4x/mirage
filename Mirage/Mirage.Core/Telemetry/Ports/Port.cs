using Mirage.Core.Exceptions;
using Mirage.Core.Interfaces;

namespace Mirage.Core.Telemetry.Ports;

/// <summary>
/// Defines the execution or sorting priority levels for telemetry ports.
/// Higher values indicate higher priority during message distribution.
/// </summary>
public enum PortPriority
{
    /// <summary>
    /// Represents the lowest priority for a telemetry port.
    /// </summary>
    Low = 25,

    /// <summary>
    /// Represents the default priority for a telemetry port.
    /// </summary>
    Normal = 50,

    /// <summary>
    /// Represents a high priority for a telemetry port.
    /// </summary>
    High = 75,

    /// <summary>
    /// Represents the highest priority for a telemetry port.
    /// </summary>
    Critical = 100,
}

/// <summary>
/// Represents a telemetry output port responsible for dispatching or
/// persisting telemetry messages.
/// </summary>
/// <remarks>
/// A port is destroyable and must no longer process messages after it
/// has been destroyed.
/// </remarks>
public interface IPort : IDestroyable
{
    /// <summary>
    /// Gets the priority assigned to the telemetry port, which determines the
    /// execution order among registered ports.
    /// </summary>
    PortPriority Priority { get; }

    /// <summary>
    /// Dispatches or processes an incoming telemetry message.
    /// </summary>
    /// <param name="message">
    /// The telemetry message to be sent or recorded.
    /// </param>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the port has already been destroyed.
    /// </exception>
    void Send(Message message);
}
