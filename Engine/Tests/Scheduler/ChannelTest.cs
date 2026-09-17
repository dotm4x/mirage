using Mirage.Common.Lifecycle;
using Mirage.Scheduler;
using Mirage.Scheduler.Interfaces;

namespace Mirage.Tests.Scheduler;

public class ChannelTest
{
    [Fact]
    public void Constructor_CreatesChannelWithDefaultValues()
    {
        var channel = new Channel("test");

        Assert.Equal("test", channel.Identifier);
        Assert.Equal(ChannelPriority.Normal, channel.Priority);
        Assert.Empty(channel.Entries);
    }

    [Fact]
    public void Constructor_WithDuplicateEntries_AddsEntryOnlyOnce()
    {
        var entry = new TestUpdatable();

        var channel = new Channel("test", entries: [entry, entry]);

        Assert.Single(channel.Entries);
    }

    [Fact]
    public void Constructor_WithEntries_AddsEntries()
    {
        var first = new TestUpdatable();
        var second = new TestUpdatable();

        var channel = new Channel("test", entries: [first, second]);

        Assert.Equal(2, channel.Entries.Count);
        Assert.Contains(first, channel.Entries);
        Assert.Contains(second, channel.Entries);
    }

    [Fact]
    public void Constructor_WithPriority_SetsPriority()
    {
        var channel = new Channel("test", ChannelPriority.High);

        Assert.Equal(ChannelPriority.High, channel.Priority);
    }

    [Fact]
    public void Destroy_ClearsEntries()
    {
        var channel = new Channel("test", entries: [new TestUpdatable(), new TestUpdatable()]);

        channel.Destroy();

        Assert.Empty(channel.Entries);
    }

    [Fact]
    public void Update_CallsOnUpdateAfterEntries()
    {
        var updateOrder = new List<string>();
        var entry = new TestUpdatable(updateOrder);
        var channel = new TestChannel(updateOrder, [entry]);

        channel.Update(0.25);

        Assert.Equal(["entry", "channel"], updateOrder);
    }

    [Fact]
    public void Update_UpdatesAllEntries()
    {
        var first = new TestUpdatable();
        var second = new TestUpdatable();
        var channel = new Channel("test", entries: [first, second]);

        channel.Update(0.25);

        Assert.Equal(1, first.UpdateCount);
        Assert.Equal(1, second.UpdateCount);
        Assert.Equal(0.25, first.LastDeltaTime);
        Assert.Equal(0.25, second.LastDeltaTime);
    }

    [Fact]
    public void Update_WhenDestroyed_Throws()
    {
        var channel = new Channel("test");

        channel.Destroy();

        Assert.Throws<DestroyedObjectException>(() => channel.Update(0.25));
    }

    private sealed class TestChannel(
        List<string> updateOrder,
        IEnumerable<IUpdatable>? entries = null
    ) : Channel("test", entries: entries)
    {
        protected override void OnUpdate(double deltaTime)
        {
            updateOrder.Add("channel");
        }
    }

    private sealed class TestUpdatable(List<string>? updateOrder = null) : IUpdatable
    {
        public double LastDeltaTime { get; private set; }
        public int UpdateCount { get; private set; }

        public void Update(double deltaTime)
        {
            UpdateCount++;
            LastDeltaTime = deltaTime;
            updateOrder?.Add("entry");
        }
    }
}
