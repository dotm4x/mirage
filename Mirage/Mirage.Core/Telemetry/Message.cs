namespace Mirage.Core.Telemetry;

/// <summary>
/// Represents the severity level or classification of a telemetry message.
/// </summary>
public enum MessageKind
{
    /// <summary>
    /// Indicates a debug message intended for development and diagnostics.
    /// </summary>
    Debug,

    /// <summary>
    /// Indicates an informational message describing normal system activity.
    /// </summary>
    Information,

    /// <summary>
    /// Indicates a warning about a potentially problematic situation.
    /// </summary>
    Warn,

    /// <summary>
    /// Indicates an error representing a failure or unexpected condition.
    /// </summary>
    Error,
}

/// <summary>
/// Defines an immutable telemetry message containing the log content,
/// origin source, severity classification, and optional metadata.
/// </summary>
/// <param name="Content">The primary text content or payload of the message.</param>
/// <param name="Source">
/// The origin component, service, or module that generated the message.
/// </param>
/// <param name="Kind">The severity kind or classification of the message.</param>
/// <param name="Metadata">
/// Optional contextual key-value metadata associated with the message.
/// </param>
public readonly record struct Message(
    string Content,
    string Source,
    MessageKind Kind,
    IReadOnlyDictionary<string, object?>? Metadata = null
);
