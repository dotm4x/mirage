using Mirage.Core.Exceptions;
using Mirage.Core.Interfaces;
using Mirage.Core.Telemetry;
using Mirage.Core.Utilities.Events;

namespace Mirage.Core.Services;

internal sealed class ServiceContainer(IEnumerable<Service> services)
{
    private readonly IReadOnlyList<Service> services = [.. services];

    public TService Get<TService>()
        where TService : Service
    {
        return services.OfType<TService>().Single();
    }

    public Service Get(string identifier)
    {
        return services.Single(service => service.Identifier == identifier);
    }
}

internal sealed class ServiceContext
{
    public required Telemetry.Telemetry Telemetry { get; init; }

    public required ServiceContainer Services { get; init; }
}

public enum ServiceState
{
    Idle,
    Starting,
    Running,
    Stopping,
}

public abstract class Service : IDestroyable
{
    public readonly string Identifier;
    public readonly IReadOnlyList<string> Dependencies;

    protected Telemetry.Telemetry Telemetry { get; private set; } = null!;
    private readonly Store<ServiceState> state = new(ServiceState.Idle);
    public ReadonlyStore<ServiceState> State { get; }
    private readonly Dictionary<string, Service> injectedDependencies = [];
    private bool injected;
    public bool Destroyed { get; private set; }

    protected Service(string identifier, IEnumerable<string>? dependencies = null)
    {
        Identifier = identifier;
        Dependencies = [.. dependencies ?? []];

        State = state.AsReadonly();
    }

    internal void Inject(ServiceContext context)
    {
        if (injected)
        {
            throw new InvalidOperationException(
                $"Service '{Identifier}' has already been injected."
            );
        }

        Telemetry = context.Telemetry;

        foreach (var dependency in Dependencies)
        {
            injectedDependencies.Add(dependency, context.Services.Get(dependency));
        }

        injected = true;

        Telemetry.Send($"Service '{Identifier}' has been injected.", Identifier, MessageKind.Debug);
    }

    protected TService Require<TService>(string name)
        where TService : Service
    {
        if (!injected)
        {
            throw new InvalidOperationException($"Service '{Identifier}' has not been injected.");
        }

        if (!injectedDependencies.TryGetValue(name, out var service))
        {
            throw new InvalidOperationException(
                $"Service '{Identifier}' requires dependency '{name}', but it was not injected."
            );
        }

        if (service is not TService typedService)
        {
            throw new InvalidOperationException(
                $"Service '{Identifier}' requires dependency '{name}' to be of type '{typeof(TService).Name}', but it is '{service.GetType().Name}'."
            );
        }

        return typedService;
    }

    protected virtual void OnStart() { }

    protected virtual void OnStop() { }

    protected virtual void OnDestroy() { }

    internal void Start()
    {
        if (!injected)
        {
            throw new InvalidOperationException(
                $"Service '{Identifier}' has not been injected, cannot stop."
            );
        }

        ServiceState currentState = state.Get();

        if (currentState != ServiceState.Idle)
        {
            throw new InvalidOperationException(
                $"Service '{Identifier}' cannot be started from state '{currentState}'."
            );
        }

        state.Set(ServiceState.Starting);

        Telemetry.Send($"Starting service '{Identifier}'.", Identifier, MessageKind.Information);

        try
        {
            OnStart();

            state.Set(ServiceState.Running);

            Telemetry.Send(
                $"Service '{Identifier}' started successfully.",
                Identifier,
                MessageKind.Information
            );
        }
        catch (Exception exception)
        {
            state.Set(ServiceState.Idle);

            Telemetry.Send(
                $"Service '{Identifier}' failed to start.",
                Identifier,
                MessageKind.Error,
                new Dictionary<string, object?> { ["Exception"] = exception }
            );

            throw;
        }
    }

    internal void Stop()
    {
        if (!injected)
        {
            throw new InvalidOperationException(
                $"Service '{Identifier}' has not been injected, cannot stop"
            );
        }

        ServiceState currentState = state.Get();

        if (currentState != ServiceState.Running)
        {
            throw new InvalidOperationException(
                $"Service '{Identifier}' cannot be stopped from state '{currentState}'."
            );
        }

        state.Set(ServiceState.Stopping);

        Telemetry.Send($"Stopping service '{Identifier}'.", Identifier, MessageKind.Information);

        try
        {
            OnStop();

            state.Set(ServiceState.Idle);

            Telemetry.Send(
                $"Service '{Identifier}' stopped successfully.",
                Identifier,
                MessageKind.Information
            );
        }
        catch (Exception exception)
        {
            state.Set(ServiceState.Idle);

            Telemetry.Send(
                $"Service '{Identifier}' failed to stop.",
                Identifier,
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
            throw new DestroyedObjectException(
                $"Service '{Identifier}' is already destroyed, cannot destroy again."
            );
        }

        if (state.Get() != ServiceState.Idle)
        {
            throw new InvalidOperationException(
                $"Service '{Identifier}' cannot be destroyed while in state '{state.Get()}'."
            );
        }

        OnDestroy();

        state.Destroy();

        Destroyed = true;

        Telemetry.Send(
            $"Service '{Identifier}' has been destroyed.",
            Identifier,
            MessageKind.Debug
        );
    }
}
