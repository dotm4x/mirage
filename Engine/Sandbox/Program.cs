using Mirage.Core;
using Mirage.Core.Telemetry;
using Mirage.Core.Telemetry.Ports;
using Mirage.Scheduler;
using Mirage.Windowing;
using OpenTK.Mathematics;

MyGame game = new();
game.Start();

internal sealed class MyGame()
    : Game(
        modules:
        [
            new Scheduler(channels: [new Channel("main")]),
            new Windowing([
                new Window(
                    title: "My Game",
                    identifier: "main",
                    size: new Vector2(1920, 720),
                    opened: true
                ),
            ]),
        ],
        telemetry: new Telemetry([new ConsolePort()])
    )
{
    private Channel MainChannel => Scheduler.Channels["main"];

    private Scheduler Scheduler => Require<Scheduler>();

    private Windowing Windowing => Require<Windowing>();

    protected override void OnStart()
    {
        MainChannel.Entries.Add(Windowing);

        Scheduler.Run();
    }
}
