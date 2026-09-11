namespace Mirage.Tests.Core.Utilities.Events;

using Mirage.Core.Exceptions;
using Mirage.Core.Utilities.Events;
using Xunit;

public class SignalTest
{
    [Fact]
    public void Fire_WhenSignalIsActive_CallsCallback()
    {
        var signal = new Signal<int>();
        var received = 0;

        signal.Connect(value => received = value);

        signal.Fire(42);

        Assert.Equal(42, received);
    }

    [Fact]
    public void Fire_WithMultipleConnections_CallsEveryCallback()
    {
        var signal = new Signal<int>();
        var received = new List<int>();

        signal.Connect(received.Add);
        signal.Connect(received.Add);

        signal.Fire(42);

        Assert.Equal([42, 42], received);
    }

    [Fact]
    public void Fire_WithMultiplePayloads_PassesEachPayload()
    {
        var signal = new Signal<int>();
        var received = new List<int>();

        signal.Connect(received.Add);

        signal.Fire(1);
        signal.Fire(2);
        signal.Fire(3);

        Assert.Equal([1, 2, 3], received);
    }

    [Fact]
    public void Fire_WhenSignalIsDestroyed_Throws()
    {
        var signal = new Signal<int>();
        signal.Destroy();

        Assert.Throws<DestroyedObjectException>(Action);
        return;

        void Action() => signal.Fire(42);
    }

    [Fact]
    public void Fire_WhenConnectionIsDisconnected_DoesNotCallCallback()
    {
        var signal = new Signal<int>();
        var calls = 0;

        var connection = signal.Connect(_ => calls++);

        connection.Disconnect();
        signal.Fire(42);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void Fire_WithPersistentConnection_CallsCallback()
    {
        var signal = new Signal<int>();
        var calls = 0;

        signal.Connect(_ => calls++, persistent: true);

        signal.Fire(42);

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Fire_WhenCallbackThrows_RethrowsException()
    {
        var signal = new Signal<int>();
        var exception = new InvalidOperationException("Test exception");

        signal.Connect(_ => throw exception);

        var result = Record.Exception(() => signal.Fire(42));

        Assert.Same(exception, result);
    }

    [Fact]
    public void Fire_WithDifferentPayloadType_PassesPayload()
    {
        var signal = new Signal<string>();
        var received = string.Empty;

        signal.Connect(value => received = value);

        signal.Fire("Hello");

        Assert.Equal("Hello", received);
    }

    [Fact]
    public void Destroy_RemovesAllConnections()
    {
        var signal = new Signal<int>();
        var calls = 0;

        signal.Connect(_ => calls++);

        signal.Destroy();
        Assert.True(signal.Destroyed);

        Assert.Throws<DestroyedObjectException>(() => signal.Fire(42));
        Assert.Equal(0, calls);
    }
}
