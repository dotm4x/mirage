using Mirage.Core.Utilities.Events;

namespace Mirage.Core.Utilities.Events;

/// <summary>
/// Represents a dispatcher for specific events, allowing listeners to be notified when the signal fires.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to the signal listeners.</typeparam>
/// <example>
/// <code>
/// var onClick = new Signal();
///
/// onClick.Connect(data =>
/// {
///     Console.WriteLine($"Clicked {data.Id} {data.Count} times");
/// });
///
/// onClick.Fire(new ClickData("btn-submit", 3));
/// </code>
/// </example>
public class Signal<TPayload> : Event<TPayload>
{
    /// <summary>
    /// Dispatches the signal, invoking all connected listener callbacks with the provided payload.
    /// </summary>
    /// <param name="payload">The value passed to each event listener.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the signal has already been destroyed.
    /// </exception>
    public void Fire(TPayload payload)
    {
        if (Destroyed)
        {
            throw new InvalidOperationException("Signal is destroyed, cannot fire");
        }

        foreach (var connection in Connections)
        {
            try
            {
                connection.Callback(payload);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Error in signal listener: {exception}");
                throw;
            }
        }
    }
}
