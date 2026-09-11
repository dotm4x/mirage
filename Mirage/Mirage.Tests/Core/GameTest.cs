namespace Mirage.Tests.Core;

using Mirage.Core;
using Mirage.Core.Exceptions;
using Mirage.Core.Modules;
using Xunit;
using CoreTelemetry = Mirage.Core.Telemetry.Telemetry;

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
    public void Start_WhenGameIsIdle_StartsAllModules()
    {
        var firstModule = new TestModule("First");
        var secondModule = new TestModule("Second");
        var game = new TestGame([firstModule, secondModule]);

        game.Start();

        Assert.Equal(GameState.Running, game.State.Get());
        Assert.Equal(ModuleState.Running, firstModule.State.Get());
        Assert.Equal(ModuleState.Running, secondModule.State.Get());
    }

    [Fact]
    public void Start_WhenGameIsRunning_Throws()
    {
        var game = new TestGame();
        game.Start();

        Assert.Throws<InvalidOperationException>(game.Start);
    }

    [Fact]
    public void Start_WhenModuleHasDependencies_StartsDependenciesFirst()
    {
        var startOrder = new List<string>();

        var dependency = new TestModule("Dependency", onStart: () => startOrder.Add("Dependency"));

        var module = new TestModule(
            "Module",
            ["Dependency"],
            onStart: () => startOrder.Add("Module")
        );

        var game = new TestGame([module, dependency]);

        game.Start();

        Assert.Equal(["Dependency", "Module"], startOrder);
    }

    [Fact]
    public void Start_InjectsModulesBeforeStarting()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        game.Start();

        Assert.True(module.WasInjected);
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
    public void Stop_WhenGameIsRunning_StopsAllRunningModules()
    {
        var firstModule = new TestModule("First");
        var secondModule = new TestModule("Second");
        var game = new TestGame([firstModule, secondModule]);

        game.Start();
        game.Stop();

        Assert.Equal(GameState.Idle, game.State.Get());
        Assert.Equal(ModuleState.Idle, firstModule.State.Get());
        Assert.Equal(ModuleState.Idle, secondModule.State.Get());
    }

    [Fact]
    public void Stop_StopsModulesInReverseDependencyOrder()
    {
        var stopOrder = new List<string>();

        var dependency = new TestModule("Dependency", onStop: () => stopOrder.Add("Dependency"));

        var module = new TestModule(
            "Module",
            ["Dependency"],
            onStop: () => stopOrder.Add("Module")
        );

        var game = new TestGame([module, dependency]);

        game.Start();
        game.Stop();

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
    public void Destroy_WhenGameIsRunning_Throws()
    {
        var game = new TestGame();
        game.Start();

        Assert.Throws<InvalidOperationException>(game.Destroy);
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

    private sealed class TestGame(
        IEnumerable<Module>? modules = null,
        CoreTelemetry? telemetry = null
    ) : Game(modules ?? [], telemetry);

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
