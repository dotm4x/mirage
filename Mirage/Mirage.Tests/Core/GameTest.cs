using Mirage.Core;
using Mirage.Core.Exceptions;
using Mirage.Core.Modules;
using Mirage.Core.Telemetry;
using Xunit;
using CoreTelemetry = Mirage.Core.Telemetry.Telemetry;

namespace Mirage.Tests.Core;

public class GameTest
{
    [Fact]
    public void Constructor_WhenNoModulesProvided_CreatesEmptyGame()
    {
        var game = new TestGame();

        Assert.Empty(game.Modules);
        Assert.Equal(GameState.Idle, game.State.Get());
        Assert.False(game.Destroyed);
    }

    [Fact]
    public void Constructor_WithModules_RegistersAllModules()
    {
        var firstModule = new TestModule("First");
        var secondModule = new TestModule("Second");

        var game = new TestGame([firstModule, secondModule]);

        Assert.Equal(2, game.Modules.Count);
        Assert.Same(firstModule, game.Modules["First"]);
        Assert.Same(secondModule, game.Modules["Second"]);
    }

    [Fact]
    public void Constructor_WithDuplicateIdentifier_Throws()
    {
        var firstModule = new TestModule("Test");
        var secondModule = new TestModule("Test");

        Assert.Throws<InvalidOperationException>(() => new TestGame([firstModule, secondModule]));
    }

    [Fact]
    public async Task Start_WhenGameIsIdle_StartsAllModules()
    {
        var firstModule = new TestModule("First");
        var secondModule = new TestModule("Second");

        var game = new TestGame([firstModule, secondModule]);

        Task startTask = StartGame(game);

        Assert.Equal(GameState.Running, game.State.Get());
        Assert.Equal(ModuleState.Running, firstModule.State.Get());
        Assert.Equal(ModuleState.Running, secondModule.State.Get());

        game.Stop();
        await startTask;
    }

    [Fact]
    public async Task Start_WhenGameIsRunning_Throws()
    {
        var game = new TestGame();

        Task startTask = StartGame(game);

        Assert.Throws<InvalidOperationException>(game.Start);

        game.Stop();
        await startTask;
    }

    [Fact]
    public async Task Start_WhenModuleHasDependencies_StartsDependenciesFirst()
    {
        var startOrder = new List<string>();

        var dependency = new TestModule("Dependency", onStart: () => startOrder.Add("Dependency"));

        var module = new TestModule(
            "Module",
            ["Dependency"],
            onStart: () => startOrder.Add("Module")
        );

        var game = new TestGame([module, dependency]);

        Task startTask = StartGame(game);

        Assert.Equal(["Dependency", "Module"], startOrder);

        game.Stop();
        await startTask;
    }

    [Fact]
    public async Task Start_InjectsModulesBeforeStarting()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        Task startTask = StartGame(game);

        Assert.True(module.WasInjected);

        game.Stop();
        await startTask;
    }

    [Fact]
    public void Start_WhenModuleFailsToStart_ReturnsGameToIdle()
    {
        var module = new TestModule(
            "Failing",
            onStart: () => throw new InvalidOperationException("Test exception")
        );

        var game = new TestGame([module]);

        Assert.Throws<InvalidOperationException>(game.Start);

        Assert.Equal(GameState.Idle, game.State.Get());
    }

    [Fact]
    public void Start_WhenModuleFailsToStart_RollsBackStartedModules()
    {
        var stopped = new List<string>();

        var firstModule = new TestModule("First", onStop: () => stopped.Add("First"));

        var secondModule = new TestModule(
            "Second",
            onStart: () => throw new InvalidOperationException("Test exception")
        );

        var game = new TestGame([firstModule, secondModule]);

        Assert.Throws<InvalidOperationException>(game.Start);

        Assert.Equal(ModuleState.Idle, firstModule.State.Get());
        Assert.Equal(["First"], stopped);
    }

    [Fact]
    public async Task Start_CallsOnUpdate()
    {
        var updates = 0;

        var game = new TestGame(onUpdate: _ => updates++, stopAfterUpdates: 1);

        Task startTask = Task.Run(game.Start);

        await startTask;

        Assert.Equal(1, updates);
        Assert.Equal(GameState.Idle, game.State.Get());
    }

    [Fact]
    public async Task Start_OnUpdateReceivesDeltaTime()
    {
        double deltaTime = -1;

        var game = new TestGame(
            onUpdate: elapsedTime => deltaTime = elapsedTime,
            stopAfterUpdates: 1
        );

        Task startTask = Task.Run(game.Start);

        await startTask;

        Assert.True(deltaTime >= 0);
        Assert.Equal(GameState.Idle, game.State.Get());
    }

    [Fact]
    public async Task Start_WhenOnUpdateThrows_ReturnsGameToIdle()
    {
        var game = new TestGame(onUpdate: _ =>
            throw new InvalidOperationException("Test exception")
        );

        Task startTask = Task.Run(game.Start);

        await Assert.ThrowsAsync<InvalidOperationException>(() => startTask);

        Assert.Equal(GameState.Running, game.State.Get());
        game.Stop();
    }

    [Fact]
    public void Start_UsesConfiguredTargetFramerate()
    {
        var game = new TestGame(targetFramerate: 60);

        Assert.Equal(60, game.TargetFramerate);
    }

    [Fact]
    public void Start_WhenTargetFramerateIsUnlimited_AllowsZero()
    {
        var game = new TestGame(targetFramerate: 0);

        Assert.Equal(0, game.TargetFramerate);
    }

    [Fact]
    public async Task Stop_WhenGameIsRunning_StopsAllRunningModules()
    {
        var firstModule = new TestModule("First");
        var secondModule = new TestModule("Second");

        var game = new TestGame([firstModule, secondModule]);

        Task startTask = StartGame(game);
        game.Stop();
        await startTask;

        Assert.Equal(GameState.Idle, game.State.Get());
        Assert.Equal(ModuleState.Idle, firstModule.State.Get());
        Assert.Equal(ModuleState.Idle, secondModule.State.Get());
    }

    [Fact]
    public async Task Stop_StopsModulesInReverseDependencyOrder()
    {
        var stopOrder = new List<string>();

        var dependency = new TestModule("Dependency", onStop: () => stopOrder.Add("Dependency"));

        var module = new TestModule(
            "Module",
            ["Dependency"],
            onStop: () => stopOrder.Add("Module")
        );

        var game = new TestGame([module, dependency]);

        Task startTask = StartGame(game);
        game.Stop();
        await startTask;

        Assert.Equal(["Module", "Dependency"], stopOrder);
    }

    [Fact]
    public void Stop_WhenGameIsIdle_Throws()
    {
        var game = new TestGame();

        Assert.Throws<InvalidOperationException>(game.Stop);
    }

    [Fact]
    public void Start_WhenDependencyIsMissing_Throws()
    {
        var module = new TestModule("Module", ["Missing"]);

        var game = new TestGame([module]);

        Assert.Throws<InvalidOperationException>(game.Start);

        Assert.Equal(GameState.Idle, game.State.Get());
    }

    [Fact]
    public void Start_WhenDependenciesFormCycle_Throws()
    {
        var firstModule = new TestModule("First", ["Second"]);
        var secondModule = new TestModule("Second", ["First"]);

        var game = new TestGame([firstModule, secondModule]);

        Assert.Throws<InvalidOperationException>(game.Start);

        Assert.Equal(GameState.Idle, game.State.Get());
    }

    [Fact]
    public void Destroy_WhenGameIsIdle_DestroysAllModules()
    {
        var firstModule = new TestModule("First");
        var secondModule = new TestModule("Second");

        var game = new TestGame([firstModule, secondModule]);

        game.Destroy();

        Assert.True(game.Destroyed);
        Assert.True(firstModule.Destroyed);
        Assert.True(secondModule.Destroyed);
        Assert.Empty(game.Modules);
    }

    [Fact]
    public async Task Destroy_WhenGameIsRunning_Throws()
    {
        var game = new TestGame();

        Task startTask = StartGame(game);

        Assert.Throws<InvalidOperationException>(game.Destroy);

        game.Stop();
        await startTask;
    }

    [Fact]
    public void Destroy_WhenAlreadyDestroyed_Throws()
    {
        var game = new TestGame();

        game.Destroy();

        Assert.Throws<DestroyedObjectException>(game.Destroy);
    }

    [Fact]
    public void Start_WhenGameIsDestroyed_Throws()
    {
        var game = new TestGame();

        game.Destroy();

        Assert.Throws<DestroyedObjectException>(game.Start);
    }

    [Fact]
    public void Stop_WhenGameIsDestroyed_Throws()
    {
        var game = new TestGame();

        game.Destroy();

        Assert.Throws<DestroyedObjectException>(game.Stop);
    }

    private static Task StartGame(TestGame game)
    {
        Task startTask = Task.Run(game.Start);

        Assert.True(
            SpinWait.SpinUntil(() => game.State.Get() == GameState.Running, TimeSpan.FromSeconds(1))
        );

        return startTask;
    }

    private sealed class TestGame(
        IEnumerable<Module>? modules = null,
        CoreTelemetry? telemetry = null,
        int targetFramerate = 0,
        Action<double>? onUpdate = null,
        int? stopAfterUpdates = null
    ) : Game(modules ?? [], telemetry: telemetry, targetFramerate: targetFramerate)
    {
        private int updates;

        protected override void OnUpdate(double deltaTime)
        {
            updates++;

            onUpdate?.Invoke(deltaTime);

            if (stopAfterUpdates.HasValue && updates >= stopAfterUpdates.Value)
            {
                Stop();
            }
        }
    }

    private sealed class TestModule(
        string identifier,
        IEnumerable<string>? dependencies = null,
        Action? onStart = null,
        Action? onStop = null
    ) : Module(identifier, dependencies)
    {
        public bool WasInjected { get; private set; }

        protected override void OnStart()
        {
            WasInjected = true;
            onStart?.Invoke();
        }

        protected override void OnStop()
        {
            onStop?.Invoke();
        }
    }
}
