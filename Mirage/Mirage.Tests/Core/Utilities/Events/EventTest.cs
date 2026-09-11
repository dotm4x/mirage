namespace Mirage.Tests.Core.Utilities.Events;

using Mirage.Core.Exceptions;
using Mirage.Core.Utilities.Events;
using Xunit;

public class EventTest
{
    [Fact]
    public void Connect_WhenEventIsActive_CallsCallbackWhenFired()
    {
        var @event = new TestEvent();
        var received = 0;

        @event.Connect(value => received = value);

        @event.Fire(42);

        Assert.Equal(42, received);
    }

    [Fact]
    public void Connect_WhenEventIsActive_ReturnsConnection()
    {
        var @event = new TestEvent();

        var connection = @event.Connect(_ => { });

        Assert.NotNull(connection);
        Assert.False(connection.Persistent);
    }

    [Fact]
    public void Connect_WithPersistentConnection_MarksConnectionAsPersistent()
    {
        var @event = new TestEvent();

        var connection = @event.Connect(_ => { }, persistent: true);

        Assert.True(connection.Persistent);
    }

    [Fact]
    public void Connect_WhenEventIsDestroyed_Throws()
    {
        var @event = new TestEvent();
        @event.Destroy();

        Assert.Throws<DestroyedObjectException>((Func<EventConnection<int>>)Action);
        return;

        EventConnection<int> Action() => @event.Connect(_ => { });
    }

    [Fact]
    public void Connection_WhenDisconnected_DoesNotReceiveEvents()
    {
        var @event = new TestEvent();
        var calls = 0;

        var connection = @event.Connect(_ => calls++);

        connection.Disconnect();
        @event.Fire(1);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void Clear_RemovesNonPersistentConnections()
    {
        var @event = new TestEvent();
        var calls = 0;

        @event.Connect(_ => calls++);
        @event.Clear();

        @event.Fire(1);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void Clear_KeepsPersistentConnections()
    {
        var @event = new TestEvent();
        var calls = 0;

        @event.Connect(_ => calls++, persistent: true);
        @event.Clear();

        @event.Fire(1);

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Clear_WithForce_RemovesPersistentConnections()
    {
        var @event = new TestEvent();
        var calls = 0;

        @event.Connect(_ => calls++, persistent: true);
        @event.Clear(force: true);

        @event.Fire(1);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void Clear_WhenEventIsDestroyed_Throws()
    {
        var @event = new TestEvent();
        @event.Destroy();

        Assert.Throws<DestroyedObjectException>(Action);
        return;

        void Action() => @event.Clear();
    }

    [Fact]
    public void Clear_WithForce_WhenEventIsDestroyed_Throws()
    {
        var @event = new TestEvent();
        @event.Destroy();

        Assert.Throws<DestroyedObjectException>(Action);
        return;

        void Action() => @event.Clear(force: true);
    }

    [Fact]
    public void AsReadonly_AllowsConnectingToEvent()
    {
        var @event = new TestEvent();
        var readonlyEvent = @event.AsReadonly();
        var received = 0;

        readonlyEvent.Connect(value => received = value);

        @event.Fire(42);

        Assert.Equal(42, received);
    }

    [Fact]
    public void AsReadonly_WhenEventIsDestroyed_CannotConnect()
    {
        var @event = new TestEvent();
        var readonlyEvent = @event.AsReadonly();

        @event.Destroy();

        Assert.Throws<DestroyedObjectException>(Action);
        return;

        void Action() => readonlyEvent.Connect(_ => { });
    }

    [Fact]
    public void Destroy_RemovesAllConnections()
    {
        var @event = new TestEvent();
        var calls = 0;

        @event.Connect(_ => calls++);
        @event.Destroy();

        Assert.True(@event.Destroyed);

        @event.Fire(1);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void Destroy_MarksEventAsDestroyed()
    {
        var @event = new TestEvent();

        @event.Destroy();

        Assert.True(@event.Destroyed);
    }

    [Fact]
    public void Destroy_WhenAlreadyDestroyed_Throws()
    {
        var @event = new TestEvent();
        @event.Destroy();

        var action = @event.Destroy;

        Assert.Throws<DestroyedObjectException>(action);
    }

    private sealed class TestEvent : Event<int>
    {
        public void Fire(int value)
        {
            foreach (var connection in Connections)
            {
                connection.Callback(value);
            }
        }
    }
}
