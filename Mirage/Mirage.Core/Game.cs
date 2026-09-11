using Mirage.Core.Exceptions;
using Mirage.Core.Interfaces;
using Mirage.Core.Services;
using Mirage.Core.Telemetry;
using Mirage.Core.Utilities.Collections;
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
    private readonly Dictionary<string, Service> services = [];

    public IReadOnlyDictionary<string, Service> Services;

    public readonly Telemetry.Telemetry Telemetry;

    private readonly Store<GameState> state = new(GameState.Idle);
    public readonly ReadonlyStore<GameState> State;

    public bool Destroyed { get; private set; }

    private IReadOnlyList<Service>? serviceOrder;

    protected Game(IEnumerable<Service> services, Telemetry.Telemetry? telemetry = null)
    {
        foreach (var service in services)
        {
            this.services[service.Identifier] = service;
        }

        Telemetry = telemetry ?? new();
        State = state.AsReadonly();

        Services = this.services;
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

        try
        {
            IReadOnlyList<Service> sortedServices = ResolveServiceOrder();

            ServiceContext context = new()
            {
                Telemetry = Telemetry,
                Services = new ServiceContainer(sortedServices),
            };

            foreach (var service in sortedServices)
            {
                service.Inject(context);
            }

            foreach (var service in sortedServices)
            {
                service.Start();
            }

            OnStart();

            serviceOrder = sortedServices;
            state.Set(GameState.Running);

            Telemetry.Send("Game is now running", "Game", MessageKind.Information);
        }
        catch (Exception exception)
        {
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

        IReadOnlyList<Service> sortedServices = serviceOrder ?? ResolveServiceOrder();

        try
        {
            for (int index = sortedServices.Count - 1; index >= 0; index--)
            {
                Service service = sortedServices[index];

                if (service.State.Get() == ServiceState.Running)
                {
                    service.Stop();
                }
            }

            OnStop();

            state.Set(GameState.Idle);

            Telemetry.Send("Game is now idle", "Game", MessageKind.Information);
        }
        catch (Exception exception)
        {
            state.Set(GameState.Idle);

            Telemetry.Send(
                "Game failed to stop",
                "Game",
                MessageKind.Error,
                new Dictionary<string, object?> { ["Exception"] = exception }
            );

            throw;
        }
    }

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

        foreach (var service in services.Values)
        {
            service.Destroy();
        }

        services.Clear();
        state.Destroy();

        Destroyed = true;

        Telemetry.Send("Game has been destroyed", "Game", MessageKind.Debug);

        Telemetry.Destroy();
    }

    private IReadOnlyList<Service> ResolveServiceOrder()
    {
        Dictionary<string, Service> servicesByIdentifier = [];

        foreach (var service in services.Values)
        {
            if (servicesByIdentifier.ContainsKey(service.Identifier))
            {
                throw new InvalidOperationException(
                    $"Duplicate service identifier found: '{service.Identifier}'"
                );
            }

            servicesByIdentifier.Add(service.Identifier, service);
        }

        List<Service> sortedServices = [];
        HashSet<string> visiting = [];
        HashSet<string> visited = [];

        void Resolve(Service service, List<string> dependencyPath)
        {
            if (visiting.Contains(service.Identifier))
            {
                string cyclePath = string.Join(" -> ", [.. dependencyPath, service.Identifier]);

                throw new InvalidOperationException(
                    $"Circular dependency detected in services: {cyclePath}"
                );
            }

            if (visited.Contains(service.Identifier))
            {
                return;
            }

            visiting.Add(service.Identifier);
            dependencyPath.Add(service.Identifier);

            foreach (var dependency in service.Dependencies)
            {
                if (!servicesByIdentifier.TryGetValue(dependency, out Service? dependencyService))
                {
                    throw new InvalidOperationException(
                        $"Service '{service.Identifier}' requires missing dependency '{dependency}'"
                    );
                }

                Resolve(dependencyService, dependencyPath);
            }

            dependencyPath.RemoveAt(dependencyPath.Count - 1);

            visiting.Remove(service.Identifier);
            visited.Add(service.Identifier);
            sortedServices.Add(service);
        }

        foreach (var service in services.Values)
        {
            if (!visited.Contains(service.Identifier))
            {
                Resolve(service, []);
            }
        }

        return sortedServices;
    }
}
