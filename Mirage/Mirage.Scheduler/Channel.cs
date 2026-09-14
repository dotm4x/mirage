using System.Collections.ObjectModel;
using Mirage.Core.Lifecycle;

namespace Mirage.Scheduler;

public class Channel : Destroyable
{
    private readonly Dictionary<string, Channel> _channels = [];

    public readonly string Identifier;

    public readonly ReadOnlyDictionary<string, Channel> Channels;

    public Channel(string identifier, IEnumerable<Channel>? channels = null)
    {
        Identifier = identifier;

        foreach (var channel in channels ?? [])
            if (!_channels.TryAdd(channel.Identifier, channel))
                throw new InvalidOperationException(
                    $"Duplicate channel identifier found: '{channel.Identifier}'"
                );

        Channels = _channels.AsReadOnly();
    }

    public void Update() { }

    protected override void OnDestroy() { }
}
