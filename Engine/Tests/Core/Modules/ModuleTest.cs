namespace Mirage.Tests.Core.Modules;

using Mirage.Core;
using Mirage.Core.Lifecycle;
using Xunit;

public class ModuleTest
{
    private static Task StartGame(TestGame game)
    {
        var startTask = Task.Run(game.Start);

        Assert.True(
            SpinWait.SpinUntil(() => game.State.Get() == GameState.Running, TimeSpan.FromSeconds(1))
        );

        return startTask;
    }

    [Fact]
    public void Constructor_CreatesModuleWithIdleState()
    {
        var module = new TestModule("Test");

        Assert.Equal("Test", module.Identifier);
        Assert.Empty(module.Dependencies);
        Assert.Equal(ModuleState.Idle, module.State.Get());
    }

    [Fact]
    public void Constructor_WithDependencies_RegistersDependencies()
    {
        var module = new TestModule("Test", ["First", "Second"]);

        Assert.Equal(["First", "Second"], module.Dependencies);
    }

    [Fact]
    public void Destroy_WhenModuleIsIdle_CallsOnDestroy()
    {
        var destroyed = false;

        var module = new TestModule("Test", onDestroy: () => destroyed = true);

        var game = new TestGame([module]);

        game.Destroy();

        Assert.True(destroyed);
    }

    [Fact]
    public void Destroy_WhenModuleIsIdle_DestroysModule()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        game.Destroy();

        Assert.True(module.Destroyed);
    }

    [Fact]
    public async Task Destroy_WhenModuleIsRunning_Throws()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        var startTask = StartGame(game);

        Assert.Throws<InvalidOperationException>(game.Destroy);

        game.Stop();
        await startTask;
    }

    [Fact]
    public void Require_WhenDependencyDoesNotExist_Throws()
    {
        var module = new TestModule("Test", ["Missing"]);
        var game = new TestGame([module]);

        Assert.Throws<InvalidOperationException>(game.Start);
    }

    [Fact]
    public async Task Require_WhenDependencyExists_ReturnsDependency()
    {
        var dependency = new TestDependencyModule("Dependency");
        var module = new TestModule("Test", ["Dependency"]);

        var game = new TestGame([module, dependency]);

        var startTask = StartGame(game);

        Assert.Same(dependency, module.GetDependency());

        game.Stop();
        await startTask;
    }

    [Fact]
    public async Task Require_WhenDependencyHasWrongType_Throws()
    {
        var dependency = new TestOtherModule("Dependency");
        var module = new TestModule("Test", ["Dependency"]);

        var game = new TestGame([module, dependency]);

        var startTask = StartGame(game);

        Assert.Throws<InvalidOperationException>(module.GetDependency);

        game.Stop();
        await startTask;
    }

    [Fact]
    public void Require_WhenModuleIsNotInjected_Throws()
    {
        var module = new TestModule("Test", ["Dependency"]);

        Assert.Throws<InvalidOperationException>(module.GetDependency);
    }

    [Fact]
    public void Start_WhenModuleIsDestroyed_Throws()
    {
        var module = new TestModule("Test");

        module.Destroy();

        var game = new TestGame([module]);

        Assert.Throws<DestroyedObjectException>(game.Start);
    }

    [Fact]
    public async Task Start_WhenModuleIsInjected_CallsOnStart()
    {
        var started = false;

        var module = new TestModule("Test", onStart: () => started = true);

        var game = new TestGame([module]);

        var startTask = StartGame(game);

        Assert.True(started);

        game.Stop();
        await startTask;
    }

    [Fact]
    public async Task Start_WhenModuleIsInjected_StartsModule()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        var startTask = StartGame(game);

        Assert.Equal(ModuleState.Running, module.State.Get());

        game.Stop();
        await startTask;
    }

    [Fact]
    public async Task Start_WhenModuleIsRunning_Throws()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        var startTask = StartGame(game);

        Assert.Throws<InvalidOperationException>(game.Start);

        game.Stop();
        await startTask;
    }

    [Fact]
    public void Start_WhenOnStartThrows_ReturnsModuleToIdle()
    {
        var module = new TestModule(
            "Test",
            onStart: () => throw new InvalidOperationException("Test exception")
        );

        var game = new TestGame([module]);

        Assert.Throws<InvalidOperationException>(game.Start);

        Assert.Equal(ModuleState.Idle, module.State.Get());
    }

    [Fact]
    public void Stop_WhenModuleIsIdle_Throws()
    {
        var game = new TestGame();

        Assert.Throws<InvalidOperationException>(game.Stop);
    }

    [Fact]
    public async Task Stop_WhenModuleIsRunning_CallsOnStop()
    {
        var stopped = false;

        var module = new TestModule("Test", onStop: () => stopped = true);

        var game = new TestGame([module]);

        var startTask = StartGame(game);
        game.Stop();
        await startTask;

        Assert.True(stopped);
    }

    [Fact]
    public async Task Stop_WhenModuleIsRunning_StopsModule()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        var startTask = StartGame(game);
        game.Stop();
        await startTask;

        Assert.Equal(ModuleState.Idle, module.State.Get());
    }

    [Fact]
    public async Task Stop_WhenOnStopThrows_ReturnsModuleToRunning()
    {
        var stopAttempts = 0;
        var module = new TestModule(
            "Test",
            onStop: () =>
            {
                if (stopAttempts++ == 0)
                    throw new InvalidOperationException("Test exception");
            }
        );

        var game = new TestGame([module]);

        var startTask = StartGame(game);

        Assert.Throws<InvalidOperationException>(game.Stop);

        Assert.Equal(ModuleState.Running, module.State.Get());

        game.Stop();
        await startTask;
    }

    private sealed class TestDependencyModule(string identifier) : Module(identifier);

    private sealed class TestGame(IEnumerable<Module>? modules = null) : Game(modules ?? []);

    private sealed class TestModule(
        string identifier,
        IEnumerable<string>? dependencies = null,
        Action? onStart = null,
        Action? onStop = null,
        Action? onDestroy = null
    ) : Module(identifier, dependencies)
    {
        protected override void OnDestroy()
        {
            onDestroy?.Invoke();
        }

        protected override void OnStart()
        {
            onStart?.Invoke();
        }

        protected override void OnStop()
        {
            onStop?.Invoke();
        }

        public TestDependencyModule GetDependency()
        {
            return Require<TestDependencyModule>("Dependency");
        }
    }

    private sealed class TestOtherModule(string identifier) : Module(identifier);
}
