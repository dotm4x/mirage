using Mirage.Core.Lifecycle;
using Mirage.Scheduler.Interfaces;

namespace Mirage.Scheduler;

/// <summary>
/// Represents the priority of a channel.
/// </summary>
public enum ChannelPriority
{
    /// <summary>
    /// Low priority.
    /// </summary>
    Low,

    /// <summary>
    /// Normal priority.
    /// </summary>
    Normal,

    /// <summary>
    /// High priority.
    /// </summary>
    High,

    /// <summary>
    /// Critical priority.
    /// </summary>
    Critical,
}

/// <summary>
/// Represents a prioritized collection of updatable entries.
/// </summary>
public class Channel : Destroyable
{
    /// <summary>
    /// Gets the updatable entries contained in this channel.
    /// </summary>
    public readonly HashSet<IUpdatable> Entries = [];

    /// <summary>
    /// Gets the unique identifier for this channel.
    /// </summary>
    public readonly string Identifier;

    /// <summary>
    /// Gets the priority of this channel.
    /// </summary>
    public readonly ChannelPriority Priority;

    /// <summary>
    /// Initializes a new instance of the <see cref="Channel"/> class.
    /// </summary>
    /// <param name="identifier">The unique identifier for the channel.</param>
    /// <param name="priority">The priority of the channel.</param>
    /// <param name="entries">The initial updatable entries in the channel.</param>
    public Channel(
        string identifier,
        ChannelPriority priority = ChannelPriority.Normal,
        IEnumerable<IUpdatable>? entries = null
    )
    {
        Identifier = identifier;
        Priority = priority;

        foreach (var entry in entries ?? [])
            Entries.Add(entry);
    }

    /// <inheritdoc cref="Destroyable.OnDestroy"/>
    protected override void OnDestroy()
    {
        Entries.Clear();
    }

    /// <summary>
    /// Called after all entries in the channel have been updated.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the previous update.</param>
    protected virtual void OnUpdate(double deltaTime) { }

    /// <summary>
    /// Updates all entries in the channel.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the previous update.</param>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the channel has already been destroyed.
    /// </exception>
    public void Update(double deltaTime)
    {
        ThrowIfDestroyed();

        foreach (var entry in Entries)
            entry.Update(deltaTime);

        OnUpdate(deltaTime);
    }
}
