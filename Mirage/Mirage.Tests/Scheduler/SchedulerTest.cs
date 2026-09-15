using Mirage.Core;
using Mirage.Scheduler;
using Mirage.Scheduler.Interfaces;

namespace Mirage.Tests.Scheduler;

public class SchedulerTest
{
    [Fact]
    public void Constructor_CreatesSchedulerWithDefaultValues()
    {
        var scheduler = new Mirage.Scheduler.Scheduler();

        Assert.Equal(60, scheduler.TargetFramerate);
        Assert.Empty(scheduler.Channels);
        Assert.Equal(0, scheduler.DeltaTime);
        Assert.Equal(0, scheduler.Framerate);
    }

    [Fact]
    public void Constructor_WithChannels_RegistersChannels()
    {
        var first = new Channel("first");
        var second = new Channel("second");

        var scheduler = new Mirage.Scheduler.Scheduler(channels: [first, second]);

        Assert.Equal(2, scheduler.Channels.Count);
        Assert.Same(first, scheduler.Channels["first"]);
        Assert.Same(second, scheduler.Channels["second"]);
    }

    [Fact]
    public void Constructor_WithDuplicateChannelIdentifier_Throws()
    {
        var first = new Channel("test");
        var second = new Channel("test");

        Assert.Throws<InvalidOperationException>(
            () => new Mirage.Scheduler.Scheduler(channels: [first, second])
        );
    }

    [Fact]
    public void Constructor_WithTargetFramerate_SetsTargetFramerate()
    {
        var scheduler = new Mirage.Scheduler.Scheduler(144);

        Assert.Equal(144, scheduler.TargetFramerate);
    }

    [Fact]
    public async Task Run_StopsWhenSchedulerStops()
    {
        var scheduler = new Mirage.Scheduler.Scheduler(0);
        var game = new TestGame(scheduler);

        game.Start();

        var runTask = Task.Run(scheduler.Run);

        Assert.True(
            SpinWait.SpinUntil(
                () => scheduler.DeltaTime > 0,
                TimeSpan.FromSeconds(1)
            )
        );

        game.Stop();

        await runTask.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Run_UpdatesChannels()
    {
        var entry = new TestUpdatable();
        var channel = new Channel("test", entries: [entry]);
        var scheduler = new Mirage.Scheduler.Scheduler(0, [channel]);
        var game = new TestGame(scheduler);

        game.Start();

        var runTask = Task.Run(scheduler.Run);

        Assert.True(
            SpinWait.SpinUntil(
                () => Volatile.Read(ref entry.UpdateCount) > 0,
                TimeSpan.FromSeconds(1)
            )
        );

        game.Stop();

        await runTask.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.True(Volatile.Read(ref entry.UpdateCount) > 0);
    }

    [Fact]
    public async Task Run_UpdatesChannelsInPriorityOrder()
    {
        var updateOrder = new List<string>();

        var low = new TestChannel(
            "low",
            ChannelPriority.Low,
            updateOrder
        );

        var normal = new TestChannel(
            "normal",
            ChannelPriority.Normal,
            updateOrder
        );

        var high = new TestChannel(
            "high",
            ChannelPriority.High,
            updateOrder
        );

        var critical = new TestChannel(
            "critical",
            ChannelPriority.Critical,
            updateOrder
        );

        var scheduler = new Mirage.Scheduler.Scheduler(
            0,
            [low, normal, high, critical]
        );

        var game = new TestGame(scheduler);

        game.Start();

        var runTask = Task.Run(scheduler.Run);

        Assert.True(
            SpinWait.SpinUntil(
                () =>
                {
                    lock (updateOrder)
                        return updateOrder.Count >= 4;
                },
                TimeSpan.FromSeconds(1)
            )
        );

        game.Stop();

        await runTask.WaitAsync(TimeSpan.FromSeconds(1));

        string[] firstFrame;

        lock (updateOrder)
            firstFrame = [.. updateOrder.Take(4)];

        Assert.Equal(
            ["critical", "high", "normal", "low"],
            firstFrame
        );
    }

    [Fact]
    public async Task Run_UpdatesDeltaTimeAndFramerate()
    {
        var scheduler = new Mirage.Scheduler.Scheduler(0);
        var game = new TestGame(scheduler);

        game.Start();

        var runTask = Task.Run(scheduler.Run);

        Assert.True(
            SpinWait.SpinUntil(
                () => scheduler.DeltaTime > 0,
                TimeSpan.FromSeconds(1)
            )
        );

        game.Stop();

        await runTask.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.True(scheduler.DeltaTime > 0);
        Assert.True(scheduler.Framerate > 0);
    }

    [Fact]
    public void Run_WhenSchedulerIsIdle_DoesNotUpdateChannels()
    {
        var entry = new TestUpdatable();
        var channel = new Channel("test", entries: [entry]);
        var scheduler = new Mirage.Scheduler.Scheduler(channels: [channel]);

        scheduler.Run();

        Assert.Equal(0, entry.UpdateCount);
    }

    private sealed class TestChannel(
        string identifier,
        ChannelPriority priority,
        List<string> updateOrder
    ) : Channel(identifier, priority)
    {
        protected override void OnUpdate(double deltaTime)
        {
            lock (updateOrder)
                updateOrder.Add(Identifier);
        }
    }

    private sealed class TestGame(params Module[] modules) : Game(modules);

    private sealed class TestUpdatable : IUpdatable
    {
        public int UpdateCount;

        public void Update(double deltaTime)
        {
            Interlocked.Increment(ref UpdateCount);
        }
    }
}