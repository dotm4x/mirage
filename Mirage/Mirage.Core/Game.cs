using Mirage.Core.Exceptions;
using Mirage.Core.Interfaces;
using Mirage.Core.Modules;
using Mirage.Core.Telemetry;
using Mirage.Core.Utilities.Events;

namespace Mirage.Core;

public enum GameState
{
    Idle,
    Starting,
    Running,
    Stopping,
}

public abstract class Game : IDestroyable
{
    private readonly Dictionary<string, Module> modules = [];

    public IReadOnlyDictionary<string, Module> Modules { get; }

    public readonly Telemetry.Telemetry Telemetry;

    private readonly Store<GameState> state = new(GameState.Idle);

    public readonly ReadonlyStore<GameState> State;

    /// <inheritdoc cref="IDestroyable.Destroyed"/>
    public bool Destroyed { get; private set; }

    private IReadOnlyList<Module>? moduleOrder;

    protected Game(IEnumerable<Module> services, Telemetry.Telemetry? telemetry = null)
    {
        foreach (var module in services)
        {
            if (!this.modules.TryAdd(module.Identifier, module))
            {
                throw new InvalidOperationException(
                    $"Duplicate module identifier found: '{module.Identifier}'"
                );
            }
        }

        Telemetry = telemetry ?? new();
        State = state.AsReadonly();

        Modules = this.modules;
    }

    protected virtual void OnStart() { }

    protected virtual void OnStop() { }

    protected virtual void OnDestroy() { }

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

        List<Module> startedServices = [];

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
                startedServices.Add(module);
            }

            OnStart();

            moduleOrder = sortedModules;

            state.Set(GameState.Running);

            Telemetry.Send("Game is now running", "Game", MessageKind.Information);
        }
        catch (Exception exception)
        {
            RollbackStartedServices(startedServices);

            state.Set(GameState.Idle);

            Telemetry.Send(
                "Game failed to start",
                "Game",
                MessageKind.Error,
                new Dictionary<string, object?> { ["Exception"] = exception }
            );

            throw;
        }
    }

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

    private void RollbackStartedServices(IReadOnlyList<Module> startedServices)
    {
        for (int index = startedServices.Count - 1; index >= 0; index--)
        {
            Module module = startedServices[index];

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

    private IReadOnlyList<Module> ResolveModuleOrder()
    {
        Dictionary<string, Module> servicesByIdentifier = [];

        foreach (var module in modules.Values)
        {
            servicesByIdentifier.Add(module.Identifier, module);
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
                if (!servicesByIdentifier.TryGetValue(dependency, out Module? dependencyService))
                {
                    throw new InvalidOperationException(
                        $"Module '{module.Identifier}' requires missing dependency '{dependency}'"
                    );
                }

                Resolve(dependencyService, dependencyPath);
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
