using Mirage.Core;
using Mirage.Core.Telemetry;
using Mirage.Core.Telemetry.Ports;
using Mirage.Scheduler;
using Mirage.Scheduler.Interfaces;

MyGame game = new();
game.Start();

internal class MyModule() : Module("MyModule")
{
    public void DoSomething()
    {
        Telemetry.Send("Doing something");
    }
}

internal class MyUpdatable : IUpdatable
{
    public void Update(double deltaTime)
    {
        Console.WriteLine($"{nameof(MyUpdatable)} Update");
    }
}

internal class MyGame()
    : Game(
        modules:
        [
            new MyModule(),
            new Scheduler(channels: [new Channel("main", entries: [new MyUpdatable()])]),
        ],
        telemetry: new Telemetry([new ConsolePort()])
    )
{
    private MyModule MyModule => Require<MyModule>();
    private Scheduler Scheduler => Require<Scheduler>();

    protected override void OnStart()
    {
        Scheduler.Run();
    }
}
