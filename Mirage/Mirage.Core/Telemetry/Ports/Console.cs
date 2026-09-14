using Mirage.Core.Lifecycle;

namespace Mirage.Core.Telemetry.Ports;

/// <summary>
/// Represents a console-based telemetry output port that formats and writes
/// telemetry messages to standard output using ANSI colors and timestamps.
/// </summary>
public sealed class ConsolePort : Destroyable, IPort
{
    private const string AnsiCyan = "\e[36m";
    private const string AnsiGreen = "\e[32m";
    private const string AnsiYellow = "\e[33m";
    private const string AnsiGray = "\e[90m";
    private const string AnsiReset = "\e[0m";

    /// <summary>
    /// Gets the priority assigned to the console port.
    /// </summary>
    public PortPriority Priority => PortPriority.Critical;

    /// <summary>
    /// Sends a telemetry message to the console.
    /// </summary>
    /// <param name="message">The telemetry message to write.</param>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the console port has already been destroyed.
    /// </exception>
    public void Send(Message message)
    {
        ThrowIfDestroyed();

        var timestamp = DateTime.Now.ToString("HH:mm:ss");

        var color = GetColor(message.Kind);

        var prefix =
            $"{AnsiGray}[{timestamp}]{AnsiReset} "
            + $"{color}[{GetKindName(message.Kind)}]{AnsiReset}";

        var source = string.IsNullOrWhiteSpace(message.Source) ? "Unknown" : message.Source;
        var sourceTag = $"{AnsiGray}[{source}]{AnsiReset}";

        var output = $"{prefix} {sourceTag} {message.Content}";

        if (message.Metadata is not null)
        {
            Console.WriteLine(output);
            Console.WriteLine($"{AnsiGray}{message.Metadata}{AnsiReset}");
        }
        else
        {
            Console.WriteLine(output);
        }
    }

    private static string GetKindName(MessageKind kind)
    {
        return kind switch
        {
            MessageKind.Debug => "DEBUG",
            MessageKind.Information => "INFO",
            MessageKind.Warn => "WARN",
            _ => "INFO",
        };
    }

    private static string GetColor(MessageKind kind)
    {
        return kind switch
        {
            MessageKind.Debug => AnsiCyan,
            MessageKind.Information => AnsiGreen,
            MessageKind.Warn => AnsiYellow,
            _ => AnsiGreen,
        };
    }
}
