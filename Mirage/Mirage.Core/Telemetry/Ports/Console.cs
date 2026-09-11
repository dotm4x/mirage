using Mirage.Core.Exceptions;
using Mirage.Core.Interfaces;
using Mirage.Core.Telemetry;

namespace Mirage.Core.Telemetry.Ports;

/// <summary>
/// Represents a console-based telemetry output port that formats and writes
/// telemetry messages to standard output using ANSI colors and timestamps.
/// </summary>
public sealed class ConsolePort : IPort
{
    public PortPriority Priority => PortPriority.Critical;

    /// <inheritdoc cref="IDestroyable.Destroyed"/>
    public bool Destroyed { get; private set; }

    public void Send(Message message)
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException("Console port is destroyed, cannot send messages");
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        string color = GetColor(message.Kind);

        string prefix =
            $"{AnsiGray}[{timestamp}]{AnsiReset} "
            + $"{color}[{GetKindName(message.Kind)}]{AnsiReset}";

        string source = string.IsNullOrWhiteSpace(message.Source) ? "Unknown" : message.Source;
        string sourceTag = $"{AnsiGray}[{source}]{AnsiReset}";

        string output = $"{prefix} {sourceTag} {message.Content}";

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

    /// <inheritdoc cref="IDestroyable.Destroy"/>
    public void Destroy()
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException(
                "Console port is already destroyed, cannot destroy again"
            );
        }

        Destroyed = true;
    }

    private static string GetKindName(MessageKind kind)
    {
        return kind switch
        {
            MessageKind.Debug => "DEBUG",
            MessageKind.Information => "INFO",
            MessageKind.Warn => "WARN",
            MessageKind.Error => "ERROR",
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
            MessageKind.Error => AnsiRed,
            _ => AnsiGreen,
        };
    }

    private const string AnsiCyan = "\x1b[36m";
    private const string AnsiGreen = "\x1b[32m";
    private const string AnsiYellow = "\x1b[33m";
    private const string AnsiRed = "\x1b[31m";
    private const string AnsiGray = "\x1b[90m";
    private const string AnsiReset = "\x1b[0m";
}
