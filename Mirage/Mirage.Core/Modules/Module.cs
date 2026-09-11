using Mirage.Core.Exceptions;
using Mirage.Core.Interfaces;
using Mirage.Core.Telemetry;
using Mirage.Core.Utilities.Events;

namespace Mirage.Core.Modules;

/// <summary>
/// Provides access to a collection of modules registered in a game.
/// </summary>
internal sealed class ModuleContainer(IEnumerable<Module> modules)
{
    private readonly IReadOnlyList<Module> modules = [.. modules];

    /// <summary>
    /// Gets a module by its concrete type.
    /// </summary>
    /// <typeparam name="TModule">The type of the module to retrieve.</typeparam>
    /// <returns>The module matching the specified type.</returns>
    public TModule Get<TModule>()
        where TModule : Module
    {
        return modules.OfType<TModule>().Single();
    }

    /// <summary>
    /// Gets a module by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the module to retrieve.</param>
    /// <returns>The module with the specified identifier.</returns>
    public Module Get(string identifier)
    {
        return modules.Single(module => module.Identifier == identifier);
    }
}

/// <summary>
/// Provides the dependencies and shared context required to initialize a module.
/// </summary>
internal sealed class ModuleContext
{
    /// <summary>
    /// Gets the telemetry manager available to the module.
    /// </summary>
    public required Telemetry.Telemetry Telemetry { get; init; }

    /// <summary>
    /// Gets the collection of modules available for dependency resolution.
    /// </summary>
    public required ModuleContainer Modules { get; init; }
}

/// <summary>
/// Represents the current lifecycle state of a module.
/// </summary>
public enum ModuleState
{
    /// <summary>
    /// Indicates that the module is idle and not currently running.
    /// </summary>
    Idle,

    /// <summary>
    /// Indicates that the module is currently starting.
    /// </summary>
    Starting,

    /// <summary>
    /// Indicates that the module is running.
    /// </summary>
    Running,

    /// <summary>
    /// Indicates that the module is currently stopping.
    /// </summary>
    Stopping,
}

/// <summary>
/// Represents a game module with a managed lifecycle and declared dependencies.
/// </summary>
/// <remarks>
/// Modules are initialized and managed by a <see cref="Game"/> instance.
/// Their dependencies are injected before the module is started.
/// </remarks>
public abstract class Module : IDestroyable
{
    /// <summary>
    /// Gets the unique module identifier.
    /// </summary>
    public readonly string Identifier;

    /// <summary>
    /// Gets the identifiers of the modules required by this module.
    /// </summary>
    public readonly IReadOnlyList<string> Dependencies;

    /// <summary>
    /// Gets the telemetry manager available to the module after injection.
    /// </summary>
    protected Telemetry.Telemetry Telemetry { get; private set; } = null!;

    private readonly Store<ModuleState> state = new(ModuleState.Idle);

    /// <summary>
    /// Gets a read-only store for the current lifecycle state of the module.
    /// </summary>
    public ReadonlyStore<ModuleState> State { get; }

    private readonly Dictionary<string, Module> injectedDependencies = [];

    private bool injected;

    /// <inheritdoc cref="IDestroyable.Destroyed"/>
    public bool Destroyed { get; private set; }

    /// <summary>
    /// Creates an instance of an <see cref="Module"/>.
    /// </summary>
    /// <param name="identifier">
    /// The unique identifier of the module.
    /// </param>
    /// <param name="dependencies">
    /// The identifiers of the modules required by this module.
    /// </param>
    protected Module(string identifier, IEnumerable<string>? dependencies = null)
    {
        Identifier = identifier;
        Dependencies = [.. dependencies ?? []];

        State = state.AsReadonly();
    }

    /// <summary>
    /// Injects the shared game context and resolves the module's dependencies.
    /// </summary>
    /// <param name="context">
    /// The context containing telemetry and registered modules.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the module has already been injected.
    /// </exception>
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

    /// <summary>
    /// Gets an injected dependency of the specified type.
    /// </summary>
    /// <typeparam name="TModule">The expected type of the dependency.</typeparam>
    /// <param name="name">The identifier of the dependency.</param>
    /// <returns>The injected dependency.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the module has not been injected, the dependency was not
    /// registered, or the dependency is not of the requested type.
    /// </exception>
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

    /// <summary>
    /// Called when the module starts.
    /// </summary>
    /// <remarks>
    /// Override this method to perform module-specific startup logic.
    /// </remarks>
    protected virtual void OnStart() { }

    /// <summary>
    /// Called when the module stops.
    /// </summary>
    /// <remarks>
    /// Override this method to perform module-specific shutdown logic.
    /// </remarks>
    protected virtual void OnStop() { }

    /// <summary>
    /// Called when the module is destroyed.
    /// </summary>
    /// <remarks>
    /// Override this method to release module-specific resources.
    /// </remarks>
    protected virtual void OnDestroy() { }

    /// <summary>
    /// Starts the module and transitions it to the running state.
    /// </summary>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the module has already been destroyed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the module has not been injected or is not idle.
    /// </exception>
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

    /// <summary>
    /// Stops the module and transitions it to the idle state.
    /// </summary>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the module has already been destroyed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the module has not been injected or is not running.
    /// </exception>
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

        if (injected)
        {
            Telemetry.Send(
                $"Module '{Identifier}' has been destroyed.",
                Identifier,
                MessageKind.Debug
            );
        }
    }
}
