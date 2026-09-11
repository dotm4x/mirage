using System.Diagnostics;
using Mirage.Core.Exceptions;
using Mirage.Core.Interfaces;
using Mirage.Core.Modules;
using Mirage.Core.Telemetry;
using Mirage.Core.Utilities.Events;

namespace Mirage.Core;

/// <summary>
/// Represents the current lifecycle state of a game.
/// </summary>
public enum GameState
{
    /// <summary>
    /// Indicates that the game is idle and not currently running.
    /// </summary>
    Idle,

    /// <summary>
    /// Indicates that the game is currently starting.
    /// </summary>
    Starting,

    /// <summary>
    /// Indicates that the game is running.
    /// </summary>
    Running,

    /// <summary>
    /// Indicates that the game is currently stopping.
    /// </summary>
    Stopping,
}

/// <summary>
/// Coordinates the lifecycle, dependency resolution, telemetry, and main update
/// loop of a game and its registered modules.
/// </summary>
/// <remarks>
/// A game manages its modules by resolving their dependencies, injecting their
/// shared context, and starting and stopping them in dependency order.
///
/// Once startup has completed successfully, <see cref="Start"/> enters the main
/// game loop and repeatedly invokes <see cref="OnUpdate(double)"/> until the game
/// is stopped.
/// </remarks>
public abstract class Game : IDestroyable
{
    private readonly Dictionary<string, Module> modules = [];

    /// <summary>
    /// Gets the modules registered in the game, indexed by identifier.
    /// </summary>
    public IReadOnlyDictionary<string, Module> Modules { get; }

    /// <summary>
    /// Gets the telemetry manager used by the game and its modules.
    /// </summary>
    public readonly Telemetry.Telemetry Telemetry;

    private readonly Store<GameState> state = new(GameState.Idle);

    /// <summary>
    /// Gets a read-only view of the game's current lifecycle state.
    /// </summary>
    public readonly ReadonlyStore<GameState> State;

    /// <summary>
    /// Gets or sets the target number of frames the game attempts to process
    /// per second. A value of <c>0</c> disables frame-rate limiting.
    /// </summary>
    public double TargetFramerate
    {
        get;
        set
        {
            if (value < 0 || double.IsNaN(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Target framerate must be zero or greater."
                );
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets the current measured framerate of the game.
    /// </summary>
    public double Framerate { get; private set; }

    /// <summary>
    /// Gets the amount of time elapsed since the previous frame, in seconds.
    /// </summary>
    public double DeltaTime { get; private set; }

    /// <inheritdoc cref="IDestroyable.Destroyed"/>
    public bool Destroyed { get; private set; }

    private IReadOnlyList<Module>? moduleOrder;

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    /// <param name="modules">
    /// The initial modules to register.
    /// </param>
    /// <param name="targetFramerate">
    /// The target number of frames the game attempts to process per second.
    /// A value of <c>0</c> disables frame-rate limiting.
    /// </param>
    /// <param name="telemetry">
    /// The telemetry manager to use, or <see langword="null"/> to create a new
    /// instance.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="targetFramerate"/> is negative or not a number.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when multiple modules have the same identifier.
    /// </exception>
    protected Game(
        IEnumerable<Module> modules,
        double targetFramerate = 60,
        Telemetry.Telemetry? telemetry = null
    )
    {
        foreach (var module in modules)
        {
            if (!this.modules.TryAdd(module.Identifier, module))
            {
                throw new InvalidOperationException(
                    $"Duplicate module identifier found: '{module.Identifier}'"
                );
            }
        }

        TargetFramerate = targetFramerate;
        Telemetry = telemetry ?? new();
        State = state.AsReadonly();

        Modules = this.modules;
    }

    /// <summary>
    /// Gets a registered module of the specified type.
    /// </summary>
    /// <typeparam name="TModule">
    /// The type of the module to retrieve.
    /// </typeparam>
    /// <returns>
    /// The registered module of the specified type.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no module or more than one module matches the specified type.
    /// </exception>
    protected TModule Require<TModule>()
        where TModule : Module
    {
        return modules.Values.OfType<TModule>().Single();
    }

    /// <summary>
    /// Called when the game has successfully started all modules.
    /// </summary>
    /// <remarks>
    /// Override this method to perform game-specific startup logic.
    /// </remarks>
    protected virtual void OnStart() { }

    /// <summary>
    /// Called once for each frame while the game is running.
    /// </summary>
    /// <param name="deltaTime">
    /// The amount of time elapsed since the previous frame, in seconds.
    /// </param>
    /// <remarks>
    /// Override this method to implement game-specific per-frame logic.
    /// </remarks>
    protected virtual void OnUpdate(double deltaTime) { }

    /// <summary>
    /// Called when the game has successfully stopped all running modules.
    /// </summary>
    /// <remarks>
    /// Override this method to perform game-specific shutdown logic.
    /// </remarks>
    protected virtual void OnStop() { }

    /// <summary>
    /// Called when the game is destroyed.
    /// </summary>
    /// <remarks>
    /// Override this method to release game-specific resources.
    /// </remarks>
    protected virtual void OnDestroy() { }

    /// <summary>
    /// Starts the game, all registered modules, and the main game loop.
    /// </summary>
    /// <remarks>
    /// Module dependencies are resolved before startup. Each module receives
    /// its dependencies through dependency injection before its startup logic
    /// is executed.
    ///
    /// After all modules have started successfully, the game enters its main
    /// update loop. The loop invokes <see cref="OnUpdate(double)"/> once per
    /// frame and attempts to maintain <see cref="TargetFramerate"/>.
    ///
    /// The method does not return while the game remains running.
    /// The game loop ends when <see cref="Stop"/> changes the game state back
    /// to <see cref="GameState.Idle"/>.
    /// </remarks>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the game has already been destroyed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the game is not idle, a module dependency is missing, or
    /// module dependencies contain a circular reference.
    /// </exception>
    public void Start()
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException("Game is destroyed, cannot start");
        }

        GameState currentState = state.Get();

        if (currentState != GameState.Idle)
        {
            throw new InvalidOperationException(
                $"Game cannot be started from state '{currentState}'"
            );
        }

        state.Set(GameState.Starting);

        Telemetry.Send("Game is starting", "Game", MessageKind.Information);

        List<Module> startedModules = [];

        try
        {
            IReadOnlyList<Module> sortedModules = ResolveModuleOrder();

            ModuleContext context = new()
            {
                Telemetry = Telemetry,
                Modules = new ModuleContainer(sortedModules),
            };

            foreach (var module in sortedModules)
            {
                module.Inject(context);
            }

            foreach (var module in sortedModules)
            {
                module.Start();
                startedModules.Add(module);
            }

            OnStart();

            moduleOrder = sortedModules;

            state.Set(GameState.Running);

            Telemetry.Send("Game is now running", "Game", MessageKind.Information);
        }
        catch (Exception exception)
        {
            RollbackStartedModules(startedModules);

            state.Set(GameState.Idle);

            Telemetry.Send(
                "Game failed to start",
                "Game",
                MessageKind.Error,
                new Dictionary<string, object?> { ["Exception"] = exception }
            );

            throw;
        }

        RunUpdateLoop();
    }

    /// <summary>
    /// Runs the main game update loop.
    /// </summary>
    /// <remarks>
    /// The loop measures the elapsed time between frames, invokes
    /// <see cref="OnUpdate(double)"/>, and waits for the remaining frame time
    /// required to approach <see cref="TargetFramerate"/>.
    ///
    /// The loop ends when the game state is no longer
    /// <see cref="GameState.Running"/>.
    /// </remarks>
    private void RunUpdateLoop()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        double previousFrameTime = stopwatch.Elapsed.TotalSeconds;

        while (state.Get() == GameState.Running)
        {
            double frameDuration = TargetFramerate > 0 ? 1.0 / TargetFramerate : 0;
            double frameStartTime = stopwatch.Elapsed.TotalSeconds;

            DeltaTime = frameStartTime - previousFrameTime;
            previousFrameTime = frameStartTime;

            Framerate = DeltaTime > 0 ? 1.0 / DeltaTime : TargetFramerate;

            OnUpdate(DeltaTime);

            double elapsedFrameTime = stopwatch.Elapsed.TotalSeconds - frameStartTime;

            double remainingFrameTime = frameDuration - elapsedFrameTime;

            if (remainingFrameTime > 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(remainingFrameTime));
            }
        }
    }

    /// <summary>
    /// Stops the game and all running modules in reverse dependency order.
    /// </summary>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the game has already been destroyed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the game is not running.
    /// </exception>
    public void Stop()
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException("Game is destroyed, cannot stop");
        }

        GameState currentState = state.Get();

        if (currentState != GameState.Running)
        {
            throw new InvalidOperationException(
                $"Game cannot be stopped from state '{currentState}'"
            );
        }

        state.Set(GameState.Stopping);

        Telemetry.Send("Game is stopping", "Game", MessageKind.Information);

        try
        {
            IReadOnlyList<Module> sortedModules = moduleOrder ?? ResolveModuleOrder();

            for (int index = sortedModules.Count - 1; index >= 0; index--)
            {
                Module module = sortedModules[index];

                if (module.State.Get() == ModuleState.Running)
                {
                    module.Stop();
                }
            }

            OnStop();

            state.Set(GameState.Idle);

            Telemetry.Send("Game is now idle", "Game", MessageKind.Information);
        }
        catch (Exception exception)
        {
            state.Set(GameState.Running);

            Telemetry.Send(
                "Game failed to stop",
                "Game",
                MessageKind.Error,
                new Dictionary<string, object?> { ["Exception"] = exception }
            );

            throw;
        }
    }

    /// <summary>
    /// Stops the modules that started successfully before a startup failure.
    /// </summary>
    /// <param name="startedModules">
    /// The modules that started successfully before the failure occurred.
    /// </param>
    private void RollbackStartedModules(IReadOnlyList<Module> startedModules)
    {
        for (int index = startedModules.Count - 1; index >= 0; index--)
        {
            Module module = startedModules[index];

            try
            {
                module.Stop();
            }
            catch (Exception exception)
            {
                Telemetry.Send(
                    $"Failed to rollback module '{module.Identifier}' after game startup failure.",
                    module.Identifier,
                    MessageKind.Error,
                    new Dictionary<string, object?> { ["Exception"] = exception }
                );
            }
        }
    }

    /// <summary>
    /// Resolves the module startup order using their declared dependencies.
    /// </summary>
    /// <returns>
    /// The modules ordered so that each module appears after its dependencies.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a required dependency is missing or a circular dependency
    /// is detected.
    /// </exception>
    private IReadOnlyList<Module> ResolveModuleOrder()
    {
        Dictionary<string, Module> modulesByIdentifier = [];

        foreach (var module in modules.Values)
        {
            modulesByIdentifier.Add(module.Identifier, module);
        }

        List<Module> sortedModules = [];
        HashSet<string> visiting = [];
        HashSet<string> visited = [];

        void Resolve(Module module, List<string> dependencyPath)
        {
            if (visiting.Contains(module.Identifier))
            {
                string cyclePath = string.Join(" -> ", [.. dependencyPath, module.Identifier]);

                throw new InvalidOperationException(
                    $"Circular dependency detected in modules: {cyclePath}"
                );
            }

            if (visited.Contains(module.Identifier))
            {
                return;
            }

            visiting.Add(module.Identifier);
            dependencyPath.Add(module.Identifier);

            foreach (var dependency in module.Dependencies)
            {
                if (!modulesByIdentifier.TryGetValue(dependency, out Module? dependencyModule))
                {
                    throw new InvalidOperationException(
                        $"Module '{module.Identifier}' requires missing dependency '{dependency}'"
                    );
                }

                Resolve(dependencyModule, dependencyPath);
            }

            dependencyPath.RemoveAt(dependencyPath.Count - 1);

            visiting.Remove(module.Identifier);
            visited.Add(module.Identifier);
            sortedModules.Add(module);
        }

        foreach (var module in modules.Values)
        {
            if (!visited.Contains(module.Identifier))
            {
                Resolve(module, []);
            }
        }

        return sortedModules;
    }

    /// <inheritdoc cref="IDestroyable.Destroy"/>
    public void Destroy()
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException("Game is already destroyed, cannot destroy again");
        }

        GameState currentState = state.Get();

        if (currentState != GameState.Idle)
        {
            throw new InvalidOperationException(
                $"Game cannot be destroyed while in state '{currentState}'"
            );
        }

        OnDestroy();

        foreach (var module in modules.Values)
        {
            module.Destroy();
        }

        modules.Clear();

        state.Destroy();

        Destroyed = true;

        Telemetry.Send("Game has been destroyed", "Game", MessageKind.Debug);

        Telemetry.Destroy();
    }
}
