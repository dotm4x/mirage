using Mirage.Common;
using Mirage.Common.Collections;
using Mirage.Common.Events;
using Mirage.Common.Primitives;

namespace Mirage.Graph;

/// <summary>
/// Coordinates node lifecycles between roots.
/// </summary>
public sealed class Graph : Module
{
    private readonly Store<Node> _activeRoot;
    private bool _restoringRoots;

    /// <summary>
    /// Initializes a new instance of the <see cref="Graph"/> class.
    /// </summary>
    /// <param name="initial">The initially selected root.</param>
    /// <param name="roots">
    /// Additional roots to register in the <see cref="Graph"/>.
    /// </param>
    public Graph(Node initial, IEnumerable<Node>? roots = null)
        : base("Graph")
    {
        ArgumentNullException.ThrowIfNull(initial);

        _activeRoot = new Store<Node>(initial);
        ActiveRoot = _activeRoot;

        Roots.OnAdd.Connect(OnRootAdded);
        Roots.OnRemove.Connect(OnRootRemoved);
        Roots.OnUpdate.Connect(OnRootUpdated);
        Roots.OnClear.Connect(OnRootsClearing);

        foreach (var root in roots ?? [])
            Roots.Add(root.Name.Get(), root);

        Roots.Add(initial.Name.Get(), initial);
    }

    /// <summary>
    /// Gets the currently selected root.
    /// </summary>
    /// <remarks>
    /// The selected root is only loaded while the graph module is running.
    /// </remarks>
    public IReadOnlyStore<Node> ActiveRoot { get; }

    /// <summary>
    /// Gets the roots registered in the graph, indexed by stable identifiers.
    /// </summary>
    /// <remarks>
    /// A root identifier is initially obtained from its name but does not
    /// automatically change if the node is renamed.
    /// </remarks>
    public ReactiveDictionary<string, Node> Roots { get; } = [];

    private void OnRootAdded(KeyValuePair<string, Node> entry)
    {
        if (_restoringRoots)
            return;

        try
        {
            ValidateRoot(entry.Key, entry.Value);
        }
        catch
        {
            _restoringRoots = true;

            try
            {
                Roots.Remove(entry.Key);
            }
            finally
            {
                _restoringRoots = false;
            }

            throw;
        }
    }

    private void OnRootRemoved(KeyValuePair<string, Node> entry)
    {
        if (_restoringRoots || !ReferenceEquals(entry.Value, _activeRoot.Get()))
            return;

        _restoringRoots = true;

        try
        {
            Roots.Add(entry.Key, entry.Value);
        }
        finally
        {
            _restoringRoots = false;
        }

        throw new InvalidOperationException(
            $"Cannot remove active root '{entry.Key}'. Switch roots first."
        );
    }

    private void OnRootUpdated(
        (KeyValuePair<string, Node> Previous, KeyValuePair<string, Node> Current) change
    )
    {
        if (_restoringRoots)
            return;

        if (ReferenceEquals(change.Previous.Value, change.Current.Value))
            return;

        try
        {
            if (ReferenceEquals(change.Previous.Value, _activeRoot.Get()))
            {
                throw new InvalidOperationException(
                    $"Cannot replace active root '{change.Previous.Key}'. Switch roots first."
                );
            }

            ValidateRoot(change.Current.Key, change.Current.Value);
        }
        catch
        {
            _restoringRoots = true;

            try
            {
                Roots[change.Previous.Key] = change.Previous.Value;
            }
            finally
            {
                _restoringRoots = false;
            }

            throw;
        }
    }

    private void OnRootsClearing(Unit _)
    {
        if (_restoringRoots || Roots.Count == 0)
            return;

        throw new InvalidOperationException(
            "Cannot clear graph roots while an active root is registered."
        );
    }

    private void SwitchLoadedRoot(Node current, Node next)
    {
        ValidateLoadableRoot(next);

        current.Unload();

        try
        {
            next.Load();
            _activeRoot.Set(next);
        }
        catch
        {
            if (!current.Loaded)
                current.Load();

            throw;
        }
    }

    private static void ValidateLoadableRoot(Node root)
    {
        if (root.Parent.Get() is not null)
        {
            throw new InvalidOperationException(
                $"{root} cannot be loaded as a root because it has a parent."
            );
        }
    }

    private void ValidateRoot(string identifier, Node root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        if (root.Parent.Get() is not null)
        {
            throw new InvalidOperationException(
                $"{root} cannot be registered as a root because it has a parent."
            );
        }

        if (root.Loaded)
        {
            throw new InvalidOperationException(
                $"{root} cannot be registered because it is already loaded."
            );
        }

        foreach (var entry in Roots)
        {
            if (entry.Key == identifier)
                continue;

            if (ReferenceEquals(entry.Value, root))
            {
                throw new InvalidOperationException(
                    $"{root} is already registered as root '{entry.Key}'."
                );
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnDestroy()
    {
        var active = _activeRoot.Get();

        if (active.Loaded)
            active.Unload();

        foreach (var root in Roots)
            root.Value.Destroy();

        _restoringRoots = true;

        try
        {
            Roots.Destroy();
        }
        finally
        {
            _restoringRoots = false;
        }

        _activeRoot.Destroy();

        base.OnDestroy();
    }

    /// <inheritdoc/>
    protected override void OnStart()
    {
        var active = _activeRoot.Get();

        ValidateLoadableRoot(active);
        active.Load();
    }

    /// <inheritdoc/>
    protected override void OnStop()
    {
        var active = _activeRoot.Get();

        if (active.Loaded)
            active.Unload();
    }

    /// <summary>
    /// Selects the root with the specified identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the root to select.</param>
    /// <remarks>
    /// When the graph is idle, the root is selected without being loaded.
    /// When the graph is running, the current root is unloaded and the selected
    /// root is loaded immediately.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="identifier"/> is empty or consists only of
    /// whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the requested root does not exist, cannot be loaded, or the
    /// graph is currently starting or stopping.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the graph has an unrecognized module state.
    /// </exception>
    public void Switch(string identifier)
    {
        ThrowIfDestroyed();
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        if (!Roots.TryGetValue(identifier, out var next))
        {
            throw new InvalidOperationException($"Root '{identifier}' does not exist.");
        }

        var current = _activeRoot.Get();

        if (ReferenceEquals(current, next))
            return;

        var state = State.Get();

        switch (state)
        {
            case ModuleState.Idle:
                ValidateLoadableRoot(next);
                _activeRoot.Set(next);
                break;

            case ModuleState.Running:
                SwitchLoadedRoot(current, next);
                break;

            case ModuleState.Starting:
            case ModuleState.Stopping:
                throw new InvalidOperationException(
                    $"Cannot switch roots while graph is in state '{state}'."
                );

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "Unknown module state."
                );
        }
    }
}
