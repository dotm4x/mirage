using System.Collections.ObjectModel;

namespace Mirage.Scheduler;

public class Scheduler
{
    private readonly Dictionary<string, Channel> _channels = [];

    public readonly ReadOnlyDictionary<string, Channel> Channels;

    public Scheduler(IEnumerable<Channel>? channels = null)
    {
        foreach (var channel in channels ?? [])
            if (!_channels.TryAdd(channel.Identifier, channel))
                throw new InvalidOperationException(
                    $"Duplicate channel identifier found: '{channel.Identifier}'"
                );

        Channels = _channels.AsReadOnly();
    }
}