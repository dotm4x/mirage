using System.Diagnostics;
using Mirage.Core.Events;
using Mirage.Core.Lifecycle;
using Mirage.Core.Telemetry;

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
public abstract class Game : Destroyable
{
    private readonly Dictionary<string, Module> _modules = [];
    private readonly Store<GameState> _state = new(GameState.Idle);
    private IReadOnlyList<Module>? _moduleOrder;

    /// <summary>
    /// Gets the modules registered in the game, indexed by identifier.
    /// </summary>
    public IReadOnlyDictionary<string, Module> Modules { get; }

    /// <summary>
    /// Gets the telemetry manager used by the game and its modules.
    /// </summary>
    protected readonly Telemetry.Telemetry Telemetry;

    /// <summary>
    /// Gets a read-only view of the game's current lifecycle state.
    /// </summary>
    public IReadOnlyStore<GameState> State { get; }

    /// <summary>
    /// Gets or sets the target number of frames the game attempts to process
    /// per second. A value of <c>0</c> disables frame-rate limiting.
    /// </summary>
    public double TargetFramerate
    {
        get;
        private init
        {
            if (value is < 0 or double.NaN)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Target framerate must be zero or greater."
                );

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
        IEnumerable<Module>? modules = null,
        double targetFramerate = 60,
        Telemetry.Telemetry? telemetry = null
    )
    {
        Telemetry = telemetry ?? new Telemetry.Telemetry();

        foreach (var module in modules ?? [])
            if (!_modules.TryAdd(module.Identifier, module))
                throw new InvalidOperationException(
                    $"Duplicate module identifier found: '{module.Identifier}'"
                );

        TargetFramerate = targetFramerate;

        Modules = _modules.AsReadOnly();
        State = _state;
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
        return _modules.Values.OfType<TModule>().Single();
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
    /// Starts the game, all registered modules, and the main game loop.
    /// </summary>
    /// <remarks>
    /// Module dependencies are resolved before startup. Each module receives
    /// its dependencies through dependency injection before its startup logic
    /// is executed.
    ///
    /// After all modules have started successfully, the game transitions to
    /// <see cref="GameState.Running"/> and <see cref="OnStart"/> is invoked.
    /// The game then enters its main update loop.
    ///
    /// The update loop invokes <see cref="OnUpdate(double)"/> once per frame
    /// and attempts to maintain <see cref="TargetFramerate"/>.
    ///
    /// The method does not return while the game remains running.
    /// The game loop ends when <see cref="Stop"/> changes the game state to
    /// <see cref="GameState.Idle"/>.
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
        ThrowIfDestroyed();

        var currentState = _state.Get();

        if (currentState != GameState.Idle)
            throw new InvalidOperationException(
                $"Game cannot be started from state '{currentState}'"
            );

        _state.Set(GameState.Starting);

        Telemetry.Send("Game is starting", "Game");

        List<Module> startedModules = [];

        try
        {
            var sortedModules = ResolveModuleOrder();

            ModuleContext context = new()
            {
                Telemetry = Telemetry,
                Modules = new ModuleContainer(sortedModules),
            };

            foreach (var module in sortedModules)
                module.Inject(context);

            foreach (var module in sortedModules)
            {
                module.Start();
                startedModules.Add(module);
            }

            _moduleOrder = sortedModules;
            _state.Set(GameState.Running);

            OnStart();

            Telemetry.Send("Game is now running", "Game");
        }
        catch (Exception)
        {
            RollbackStartedModules(startedModules);

            _state.Set(GameState.Idle);

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
        var stopwatch = Stopwatch.StartNew();

        var previousFrameTime = stopwatch.Elapsed.TotalSeconds;

        while (_state.Get() == GameState.Running)
        {
            var frameDuration = TargetFramerate > 0 ? 1.0 / TargetFramerate : 0;
            var frameStartTime = stopwatch.Elapsed.TotalSeconds;

            DeltaTime = frameStartTime - previousFrameTime;
            previousFrameTime = frameStartTime;

            Framerate = DeltaTime > 0 ? 1.0 / DeltaTime : TargetFramerate;

            OnUpdate(DeltaTime);

            var elapsedFrameTime = stopwatch.Elapsed.TotalSeconds - frameStartTime;

            var remainingFrameTime = frameDuration - elapsedFrameTime;

            if (remainingFrameTime > 0)
                Thread.Sleep(TimeSpan.FromSeconds(remainingFrameTime));
        }
    }

    /// <summary>
    /// Stops the game and all running modules in reverse dependency order.
    /// </summary>
    /// <remarks>
    /// The game transitions to <see cref="GameState.Stopping"/> before its
    /// modules are stopped.
    ///
    /// Modules are stopped in reverse dependency order. After all running
    /// modules have stopped successfully, <see cref="OnStop"/> is invoked
    /// while the game remains in the stopping state.
    ///
    /// Once shutdown logic has completed successfully, the game transitions
    /// to <see cref="GameState.Idle"/>.
    ///
    /// If stopping fails, the game returns to
    /// <see cref="GameState.Running"/> and the exception is rethrown.
    /// </remarks>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the game has already been destroyed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the game is not running.
    /// </exception>
    public void Stop()
    {
        ThrowIfDestroyed();

        var currentState = _state.Get();

        if (currentState != GameState.Running)
            throw new InvalidOperationException(
                $"Game cannot be stopped from state '{currentState}'"
            );

        _state.Set(GameState.Stopping);

        Telemetry.Send("Game is stopping", "Game");

        try
        {
            var sortedModules = _moduleOrder ?? ResolveModuleOrder();

            for (var index = sortedModules.Count - 1; index >= 0; index--)
            {
                var module = sortedModules[index];

                if (module.State.Get() == ModuleState.Running)
                    module.Stop();
            }

            OnStop();

            _state.Set(GameState.Idle);

            Telemetry.Send("Game is now idle", "Game");
        }
        catch (Exception)
        {
            _state.Set(GameState.Running);

            throw;
        }
    }

    /// <summary>
    /// Stops the modules that started successfully before a startup failure.
    /// </summary>
    /// <param name="startedModules">
    /// The modules that started successfully before the failure occurred.
    /// </param>
    private static void RollbackStartedModules(IReadOnlyList<Module> startedModules)
    {
        for (var index = startedModules.Count - 1; index >= 0; index--)
        {
            var module = startedModules[index];

            try
            {
                module.Stop();
            }
            catch (Exception)
            {
                // ignored
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

        foreach (var module in _modules.Values)
            modulesByIdentifier.Add(module.Identifier, module);

        List<Module> sortedModules = [];
        HashSet<string> visiting = [];
        HashSet<string> visited = [];

        foreach (
            var module in _modules.Values.Where(module => !visited.Contains(module.Identifier))
        )
            Resolve(module, []);

        return sortedModules;

        void Resolve(Module module, List<string> dependencyPath)
        {
            if (visiting.Contains(module.Identifier))
            {
                var cyclePath = string.Join(" -> ", [.. dependencyPath, module.Identifier]);

                throw new InvalidOperationException(
                    $"Circular dependency detected in modules: {cyclePath}"
                );
            }

            if (visited.Contains(module.Identifier))
                return;

            visiting.Add(module.Identifier);
            dependencyPath.Add(module.Identifier);

            foreach (var dependency in module.Dependencies)
            {
                if (!modulesByIdentifier.TryGetValue(dependency, out var dependencyModule))
                    throw new InvalidOperationException(
                        $"Module '{module.Identifier}' requires missing dependency '{dependency}'"
                    );

                Resolve(dependencyModule, dependencyPath);
            }

            dependencyPath.RemoveAt(dependencyPath.Count - 1);

            visiting.Remove(module.Identifier);
            visited.Add(module.Identifier);
            sortedModules.Add(module);
        }
    }

    /// <inheritdoc cref="Destroyable.OnDestroy"/>
    protected override void OnDestroy()
    {
        var currentState = _state.Get();

        if (currentState != GameState.Idle)
            throw new InvalidOperationException(
                $"Game cannot be destroyed while in state '{currentState}'"
            );

        foreach (var module in _modules.Values)
            module.Destroy();

        _modules.Clear();

        _state.Destroy();

        Telemetry.Send("Game has been destroyed", "Game", MessageKind.Debug);

        Telemetry.Destroy();
    }
}
