namespace Mirage.Tests.Core.Utilities.Modules;

using Mirage.Core;
using Mirage.Core.Exceptions;
using Mirage.Core.Modules;
using Xunit;

public class ModuleTest
{
    [Fact]
    public void Constructor_CreatesModuleWithIdleState()
    {
        var module = new TestModule("Test");

        Assert.Equal("Test", module.Identifier);
        Assert.Empty(module.Dependencies);
        Assert.Equal(ModuleState.Idle, module.State.Get());
        Assert.False(module.Destroyed);
    }

    [Fact]
    public void Constructor_WithDependencies_RegistersDependencies()
    {
        var module = new TestModule("Test", ["First", "Second"]);

        Assert.Equal(["First", "Second"], module.Dependencies);
    }

    [Fact]
    public void Start_WhenModuleIsInjected_StartsModule()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        game.Start();

        Assert.Equal(ModuleState.Running, module.State.Get());
    }

    [Fact]
    public void Start_WhenModuleIsInjected_CallsOnStart()
    {
        var started = false;

        var module = new TestModule("Test", onStart: () => started = true);

        var game = new TestGame([module]);

        game.Start();

        Assert.True(started);
    }

    [Fact]
    public void Start_WhenModuleIsRunning_Throws()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        game.Start();

        Assert.Throws<InvalidOperationException>(game.Start);
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
    public void Stop_WhenModuleIsRunning_StopsModule()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        game.Start();
        game.Stop();

        Assert.Equal(ModuleState.Idle, module.State.Get());
    }

    [Fact]
    public void Stop_WhenModuleIsRunning_CallsOnStop()
    {
        var stopped = false;

        var module = new TestModule("Test", onStop: () => stopped = true);

        var game = new TestGame([module]);

        game.Start();
        game.Stop();

        Assert.True(stopped);
    }

    [Fact]
    public void Stop_WhenModuleIsIdle_Throws()
    {
        var game = new TestGame();

        Assert.Throws<InvalidOperationException>(game.Stop);
    }

    [Fact]
    public void Stop_WhenOnStopThrows_ReturnsModuleToRunning()
    {
        var module = new TestModule(
            "Test",
            onStop: () => throw new InvalidOperationException("Test exception")
        );

        var game = new TestGame([module]);

        game.Start();

        Assert.Throws<InvalidOperationException>(game.Stop);

        Assert.Equal(ModuleState.Running, module.State.Get());
    }

    [Fact]
    public void Require_WhenDependencyExists_ReturnsDependency()
    {
        var dependency = new TestDependencyModule("Dependency");
        var module = new TestModule("Test", ["Dependency"]);

        var game = new TestGame([module, dependency]);

        game.Start();

        Assert.Same(dependency, module.GetDependency());
    }

    [Fact]
    public void Require_WhenDependencyDoesNotExist_Throws()
    {
        var module = new TestModule("Test", ["Missing"]);
        var game = new TestGame([module]);

        Assert.Throws<InvalidOperationException>(game.Start);
    }

    [Fact]
    public void Require_WhenDependencyHasWrongType_Throws()
    {
        var dependency = new TestOtherModule("Dependency");
        var module = new TestModule("Test", ["Dependency"]);

        var game = new TestGame([module, dependency]);

        game.Start();

        Assert.Throws<InvalidOperationException>(module.GetDependency);
    }

    [Fact]
    public void Require_WhenModuleIsNotInjected_Throws()
    {
        var module = new TestModule("Test", ["Dependency"]);

        Assert.Throws<InvalidOperationException>(module.GetDependency);
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
    public void Destroy_WhenModuleIsIdle_CallsOnDestroy()
    {
        var destroyed = false;

        var module = new TestModule("Test", onDestroy: () => destroyed = true);

        var game = new TestGame([module]);

        game.Destroy();

        Assert.True(destroyed);
    }

    [Fact]
    public void Destroy_WhenModuleIsRunning_Throws()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        game.Start();

        Assert.Throws<InvalidOperationException>(game.Destroy);
    }

    [Fact]
    public void Destroy_WhenAlreadyDestroyed_Throws()
    {
        var module = new TestModule("Test");
        var game = new TestGame([module]);

        game.Destroy();

        Assert.Throws<DestroyedObjectException>(module.Destroy);
    }

    [Fact]
    public void Start_WhenModuleIsDestroyed_Throws()
    {
        var module = new TestModule("Test");

        module.Destroy();

        var game = new TestGame([module]);

        Assert.Throws<DestroyedObjectException>(game.Start);
    }

    private sealed class TestGame(IEnumerable<Module>? modules = null) : Game(modules ?? []);

    private sealed class TestModule(
        string identifier,
        IEnumerable<string>? dependencies = null,
        Action? onStart = null,
        Action? onStop = null,
        Action? onDestroy = null
    ) : Module(identifier, dependencies)
    {
        protected override void OnStart()
        {
            onStart?.Invoke();
        }

        protected override void OnStop()
        {
            onStop?.Invoke();
        }

        protected override void OnDestroy()
        {
            onDestroy?.Invoke();
        }

        public TestDependencyModule GetDependency()
        {
            return Require<TestDependencyModule>("Dependency");
        }
    }

    private sealed class TestDependencyModule(string identifier) : Module(identifier);

    private sealed class TestOtherModule(string identifier) : Module(identifier);
}
