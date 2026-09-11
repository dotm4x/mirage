namespace Mirage.Tests.Core.Telemetry;

using Mirage.Core.Exceptions;
using Mirage.Core.Telemetry;
using Mirage.Core.Telemetry.Ports;
using Xunit;

public class TelemetryTest
{
    [Fact]
    public void Constructor_WhenNoPortsProvided_CreatesEmptyTelemetry()
    {
        var telemetry = new Telemetry();

        Assert.Empty(telemetry.Ports);
        Assert.False(telemetry.Destroyed);
    }

    [Fact]
    public void Constructor_WithPorts_RegistersAllPorts()
    {
        var firstPort = new TestPort();
        var secondPort = new TestPort();
        var ports = new IPort[] { firstPort, secondPort };

        var telemetry = new Telemetry(ports);

        Assert.Equal(2, telemetry.Ports.Count);
        Assert.Equal(ports, telemetry.Ports.ToArray());
    }

    [Fact]
    public void Send_WithMessage_SendsMessageToAllPorts()
    {
        var firstPort = new TestPort();
        var secondPort = new TestPort();
        var telemetry = new Telemetry([firstPort, secondPort]);
        var message = new Message();

        telemetry.Send(message);

        Assert.Equal([message], firstPort.Messages);
        Assert.Equal([message], secondPort.Messages);
    }

    [Fact]
    public void Send_ReturnsTheSentMessage()
    {
        var telemetry = new Telemetry();
        var message = new Message();

        var result = telemetry.Send(message);

        Assert.Equal(message, result);
    }

    [Fact]
    public void Send_SendsPortsInDescendingPriorityOrder()
    {
        var sendOrder = new List<TestPort>();

        var lowPriorityPort = new TestPort(PortPriority.Low, sendOrder);
        var highPriorityPort = new TestPort(PortPriority.High, sendOrder);
        var normalPriorityPort = new TestPort(PortPriority.Normal, sendOrder);

        var telemetry = new Telemetry([lowPriorityPort, highPriorityPort, normalPriorityPort]);

        telemetry.Send(new Message());

        Assert.Equal([highPriorityPort, normalPriorityPort, lowPriorityPort], sendOrder);
    }

    [Fact]
    public void Send_WhenMessageIsSent_FiresOnSend()
    {
        var telemetry = new Telemetry();
        var received = new List<Message>();
        var message = new Message();

        telemetry.OnSend.Connect(received.Add);

        telemetry.Send(message);

        Assert.Equal([message], received);
    }

    [Fact]
    public void Send_WhenMessageIsSent_FiresOnSendAfterAllPorts()
    {
        var events = new List<string>();

        var firstPort = new TestPort(onSend: _ => events.Add("Port"));
        var secondPort = new TestPort(onSend: _ => events.Add("Port"));
        var telemetry = new Telemetry([firstPort, secondPort]);

        telemetry.OnSend.Connect(_ => events.Add("OnSend"));

        telemetry.Send(new Message());

        Assert.Equal(["Port", "Port", "OnSend"], events);
    }

    [Fact]
    public void Send_WhenDestroyed_Throws()
    {
        var telemetry = new Telemetry();
        telemetry.Destroy();

        Assert.Throws<DestroyedObjectException>(Action);
        return;

        void Action() => telemetry.Send(new Message());
    }

    [Fact]
    public void Destroy_DestroysAllPortsAndMarksTelemetryAsDestroyed()
    {
        var firstPort = new TestPort();
        var secondPort = new TestPort();
        var telemetry = new Telemetry([firstPort, secondPort]);

        telemetry.Destroy();

        Assert.True(telemetry.Destroyed);
        Assert.True(firstPort.Destroyed);
        Assert.True(secondPort.Destroyed);
    }

    [Fact]
    public void Destroy_DestroysPortsGroup()
    {
        var telemetry = new Telemetry([new TestPort()]);

        telemetry.Destroy();

        Assert.True(telemetry.Ports.Destroyed);
        Assert.Empty(telemetry.Ports);
    }

    [Fact]
    public void Destroy_WhenAlreadyDestroyed_Throws()
    {
        var telemetry = new Telemetry();
        telemetry.Destroy();

        var action = telemetry.Destroy;

        Assert.Throws<DestroyedObjectException>(action);
    }

    private sealed class TestPort(
        PortPriority priority = PortPriority.Normal,
        List<TestPort>? sendOrder = null,
        Action<Message>? onSend = null
    ) : IPort
    {
        public PortPriority Priority { get; } = priority;

        public bool Destroyed { get; private set; }

        public List<Message> Messages { get; } = [];

        public void Send(Message message)
        {
            Messages.Add(message);
            sendOrder?.Add(this);
            onSend?.Invoke(message);
        }

        public void Destroy()
        {
            if (Destroyed)
                throw new InvalidOperationException();

            Destroyed = true;
        }
    }
}
