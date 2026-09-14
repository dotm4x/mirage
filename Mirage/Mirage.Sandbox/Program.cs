using Mirage.Core;
using Mirage.Core.Telemetry;
using Mirage.Core.Telemetry.Ports;

MyGame game = new();
game.Start();

internal class MyModule() : Module("MyModule")
{
    public void DoSomething()
    {
        Telemetry.Send("Doing something");
    }
}

internal class MyGame() : Game([new MyModule()], 0, new Telemetry([new ConsolePort()]))
{
    private MyModule MyModule => Require<MyModule>();

    protected override void OnStart() { }

    protected override void OnUpdate(double deltaTime)
    {
        Telemetry.Send("Hola");
    }
}
