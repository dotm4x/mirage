using Mirage.Core.Exceptions;
using Mirage.Core.Interfaces;
using Mirage.Core.Telemetry;
using Mirage.Core.Utilities.Events;

namespace Mirage.Core.Modules;

internal sealed class ModuleContainer(IEnumerable<Module> modules)
{
    private readonly IReadOnlyList<Module> modules = [.. modules];

    public TModule Get<TModule>()
        where TModule : Module
    {
        return modules.OfType<TModule>().Single();
    }

    public Module Get(string identifier)
    {
        return modules.Single(module => module.Identifier == identifier);
    }
}

internal sealed class ModuleContext
{
    public required Telemetry.Telemetry Telemetry { get; init; }

    public required ModuleContainer Modules { get; init; }
}

public enum ModuleState
{
    Idle,
    Starting,
    Running,
    Stopping,
}

public abstract class Module : IDestroyable
{
    public readonly string Identifier;
    public readonly IReadOnlyList<string> Dependencies;

    protected Telemetry.Telemetry Telemetry { get; private set; } = null!;

    private readonly Store<ModuleState> state = new(ModuleState.Idle);

    public ReadonlyStore<ModuleState> State { get; }

    private readonly Dictionary<string, Module> injectedDependencies = [];

    private bool injected;

    /// <inheritdoc cref="IDestroyable.Destroyed"/>
    public bool Destroyed { get; private set; }

    protected Module(string identifier, IEnumerable<string>? dependencies = null)
    {
        Identifier = identifier;
        Dependencies = [.. dependencies ?? []];

        State = state.AsReadonly();
    }

    internal void Inject(ModuleContext context)
    {
        if (injected)
        {
            throw new InvalidOperationException(
                $"Module '{Identifier}' has already been injected."
            );
        }

        Telemetry = context.Telemetry;

        foreach (var dependency in Dependencies)
        {
            injectedDependencies.Add(dependency, context.Modules.Get(dependency));
        }

        injected = true;

        Telemetry.Send($"Module '{Identifier}' has been injected.", Identifier, MessageKind.Debug);
    }

    protected TModule Require<TModule>(string name)
        where TModule : Module
    {
        if (!injected)
        {
            throw new InvalidOperationException($"Module '{Identifier}' has not been injected.");
        }

        if (!injectedDependencies.TryGetValue(name, out var module))
        {
            throw new InvalidOperationException(
                $"Module '{Identifier}' requires dependency '{name}', but it was not injected."
            );
        }
        if (module is not TModule typedModule)
        {
            throw new InvalidOperationException(
                $"Module '{Identifier}' requires dependency '{name}' to be of type '{typeof(TModule).Name}', but it is '{module.GetType().Name}'."
            );
        }

        return typedModule;
    }

    protected virtual void OnStart() { }

    protected virtual void OnStop() { }

    protected virtual void OnDestroy() { }

    internal void Start()
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException(
                $"Module '{Identifier}' is destroyed, cannot start."
            );
        }

        if (!injected)
        {
            throw new InvalidOperationException(
                $"Module '{Identifier}' has not been injected, cannot start."
            );
        }

        ModuleState currentState = state.Get();

        if (currentState != ModuleState.Idle)
        {
            throw new InvalidOperationException(
                $"Module '{Identifier}' cannot be started from state '{currentState}'."
            );
        }

        state.Set(ModuleState.Starting);

        Telemetry.Send($"Starting module '{Identifier}'.", Identifier, MessageKind.Information);

        try
        {
            OnStart();

            state.Set(ModuleState.Running);

            Telemetry.Send(
                $"Module '{Identifier}' started successfully.",
                Identifier,
                MessageKind.Information
            );
        }
        catch (Exception exception)
        {
            state.Set(ModuleState.Idle);

            Telemetry.Send(
                $"Module '{Identifier}' failed to start.",
                Identifier,
                MessageKind.Error,
                new Dictionary<string, object?> { ["Exception"] = exception }
            );

            throw;
        }
    }

    internal void Stop()
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException($"Module '{Identifier}' is destroyed, cannot stop.");
        }

        if (!injected)
        {
            throw new InvalidOperationException(
                $"Module '{Identifier}' has not been injected, cannot stop."
            );
        }

        ModuleState currentState = state.Get();

        if (currentState != ModuleState.Running)
        {
            throw new InvalidOperationException(
                $"Module '{Identifier}' cannot be stopped from state '{currentState}'."
            );
        }

        state.Set(ModuleState.Stopping);

        Telemetry.Send($"Stopping module '{Identifier}'.", Identifier, MessageKind.Information);

        try
        {
            OnStop();

            state.Set(ModuleState.Idle);

            Telemetry.Send(
                $"Module '{Identifier}' stopped successfully.",
                Identifier,
                MessageKind.Information
            );
        }
        catch (Exception exception)
        {
            state.Set(ModuleState.Running);

            Telemetry.Send(
                $"Module '{Identifier}' failed to stop.",
                Identifier,
                MessageKind.Error,
                new Dictionary<string, object?> { ["Exception"] = exception }
            );

            throw;
        }
    }

    /// <inheritdoc cref="IDestroyable.Destroy"/>
    public void Destroy()
    {
        if (Destroyed)
        {
            throw new DestroyedObjectException(
                $"Module '{Identifier}' is already destroyed, cannot destroy again."
            );
        }

        if (state.Get() != ModuleState.Idle)
        {
            throw new InvalidOperationException(
                $"Module '{Identifier}' cannot be destroyed while in state '{state.Get()}'."
            );
        }

        OnDestroy();

        state.Destroy();

        Destroyed = true;

        Telemetry.Send($"Module '{Identifier}' has been destroyed.", Identifier, MessageKind.Debug);
    }
}
