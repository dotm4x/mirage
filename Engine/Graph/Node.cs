using Mirage.Common.Collections;
using Mirage.Common.Events;
using Mirage.Common.Lifecycle;

namespace Mirage.Graph;

/// <summary>
/// Represents a collection of <see cref="Node"/> instances belonging to a parent node.
/// </summary>
/// <param name="owner">The node that owns this collection.</param>
/// <remarks>
/// In addition to the standard <see cref="Group{TItem}"/> collection operations,
/// this class provides methods for locating nodes by identifier, name, path, or tag.
/// Searches can optionally include descendant nodes recursively.
/// </remarks>
public class NodeGroup(Node owner) : Group<Node>
{
    private readonly Node _owner = owner;

    /// <summary>
    /// Gets a node by its unique identifier.
    /// </summary>
    /// <param name="identifier">The unique identifier of the node to locate.</param>
    /// <param name="recursive">
    /// Whether to search descendant nodes recursively.
    /// </param>
    /// <returns>
    /// The node with the specified identifier, or <see langword="null"/> if no
    /// matching node was found.
    /// </returns>
    public Node? GetByIdentifier(Guid identifier, bool recursive = false)
    {
        foreach (var node in this)
        {
            if (node.Identifier == identifier)
                return node;

            if (!recursive)
                continue;

            var result = node.Subnodes.GetByIdentifier(identifier, true);

            if (result is not null)
                return result;
        }

        return null;
    }

    /// <summary>
    /// Gets a node of the specified type by its unique identifier.
    /// </summary>
    /// <typeparam name="TNode">
    /// The type of node to locate.
    /// </typeparam>
    /// <param name="identifier">The unique identifier of the node to locate.</param>
    /// <param name="recursive">
    /// Whether to search descendant nodes recursively.
    /// </param>
    /// <returns>
    /// The matching node, or <see langword="null"/> if no node with the specified
    /// identifier and type was found.
    /// </returns>
    public TNode? GetByIdentifier<TNode>(Guid identifier, bool recursive = false)
        where TNode : Node
    {
        foreach (var node in this)
        {
            if (node.Identifier == identifier && node is TNode typedNode)
                return typedNode;

            if (!recursive)
                continue;

            var result = node.Subnodes.GetByIdentifier<TNode>(identifier, true);

            if (result is not null)
                return result;
        }

        return null;
    }

    /// <summary>
    /// Gets a node by its name.
    /// </summary>
    /// <param name="name">The name of the node to locate.</param>
    /// <param name="recursive">
    /// Whether to search descendant nodes recursively.
    /// </param>
    /// <returns>
    /// The first node with the specified name, or <see langword="null"/> if no
    /// matching node was found.
    /// </returns>
    public Node? GetByName(string name, bool recursive = false)
    {
        foreach (var node in this)
        {
            if (node.Name.Get() == name)
                return node;

            if (!recursive)
                continue;

            var result = node.Subnodes.GetByName(name, true);

            if (result is not null)
                return result;
        }

        return null;
    }

    /// <summary>
    /// Gets a node of the specified type by its name.
    /// </summary>
    /// <typeparam name="TNode">
    /// The type of node to locate.
    /// </typeparam>
    /// <param name="name">The name of the node to locate.</param>
    /// <param name="recursive">
    /// Whether to search descendant nodes recursively.
    /// </param>
    /// <returns>
    /// The first matching node, or <see langword="null"/> if no node with the
    /// specified name and type was found.
    /// </returns>
    public TNode? GetByName<TNode>(string name, bool recursive = false)
        where TNode : Node
    {
        foreach (var node in this)
        {
            if (node.Name.Get() == name && node is TNode typedNode)
                return typedNode;

            if (!recursive)
                continue;

            var result = node.Subnodes.GetByName<TNode>(name, true);

            if (result is not null)
                return result;
        }

        return null;
    }

    /// <summary>
    /// Gets a node using a path relative to the owner of this collection.
    /// </summary>
    /// <param name="path">
    /// The path to resolve. A path is composed of node names separated by
    /// <c>/</c>. The <c>.</c> segment refers to the current node and the
    /// <c>..</c> segment refers to its parent.
    /// </param>
    /// <returns>
    /// The node resolved by the path, or <see langword="null"/> if the path
    /// cannot be resolved.
    /// </returns>
    /// <remarks>
    /// Path resolution begins at the node that owns this collection.
    /// </remarks>
    public Node? GetByPath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var current = _owner;

        foreach (var segment in segments)
        {
            switch (segment)
            {
                case ".":
                    break;

                case "..":
                    current = current.Parent.Get();
                    break;

                default:
                    current = current.Subnodes.GetByName(segment);
                    break;
            }

            if (current is null)
                return null;
        }

        return current;
    }

    /// <summary>
    /// Gets a node of the specified type using a path relative to the owner
    /// of this collection.
    /// </summary>
    /// <typeparam name="TNode">
    /// The type of node to locate.
    /// </typeparam>
    /// <param name="path">
    /// The path to resolve. A path is composed of node names separated by
    /// <c>/</c>.
    /// </param>
    /// <returns>
    /// The node resolved by the path if it is of the requested type;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public TNode? GetByPath<TNode>(string path)
        where TNode : Node
    {
        return GetByPath(path) as TNode;
    }

    /// <summary>
    /// Gets all nodes containing the specified tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <param name="recursive">
    /// Whether to search descendant nodes recursively.
    /// </param>
    /// <returns>
    /// An enumerable sequence containing all matching nodes.
    /// </returns>
    /// <remarks>
    /// When <paramref name="recursive"/> is <see langword="false"/>, only direct
    /// subnodes are searched.
    /// </remarks>
    public IEnumerable<Node> GetByTag(string tag, bool recursive = false)
    {
        foreach (var node in this)
        {
            if (node.Tags.Contains(tag))
                yield return node;

            if (!recursive)
                continue;

            foreach (var result in node.Subnodes.GetByTag(tag, true))
                yield return result;
        }
    }

    /// <summary>
    /// Gets all nodes of the specified type containing the specified tag.
    /// </summary>
    /// <typeparam name="TNode">
    /// The type of node to locate.
    /// </typeparam>
    /// <param name="tag">The tag to search for.</param>
    /// <param name="recursive">
    /// Whether to search descendant nodes recursively.
    /// </param>
    /// <returns>
    /// An enumerable sequence containing all matching nodes of the requested type.
    /// </returns>
    /// <remarks>
    /// When <paramref name="recursive"/> is <see langword="false"/>, only direct
    /// subnodes are searched.
    /// </remarks>
    public IEnumerable<TNode> GetByTag<TNode>(string tag, bool recursive = false)
        where TNode : Node
    {
        foreach (var node in this)
        {
            if (node is TNode typedNode && node.Tags.Contains(tag))
                yield return typedNode;

            if (!recursive)
                continue;

            foreach (var result in node.Subnodes.GetByTag<TNode>(tag, true))
                yield return result;
        }
    }
}

/// <summary>
/// Represents a node in a hierarchical graph.
/// </summary>
/// <remarks>
/// A node can have a parent, contain subnodes, and participate in the graph
/// lifecycle through loading, unloading, and destruction.
///
/// Nodes are identified independently by a unique <see cref="Identifier"/> and
/// a mutable human-readable <see cref="Name"/>.
/// </remarks>
public class Node : Destroyable
{
    /// <summary>
    /// Gets the unique identifier of the node.
    /// </summary>
    public readonly Guid Identifier = Guid.NewGuid();

    /// <summary>
    /// Gets the mutable name of the node.
    /// </summary>
    public readonly Store<string> Name;

    /// <summary>
    /// Gets the parent node of this node.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> when the node is a root node.
    /// Changes to this store are automatically synchronized with the parent's
    /// <see cref="Subnodes"/> collection.
    /// </remarks>
    public readonly Store<Node?> Parent;

    /// <summary>
    /// Gets a value indicating whether the node persists independently of
    /// recursive parent lifecycle operations.
    /// </summary>
    /// <remarks>
    /// Persistent nodes are not automatically loaded or unloaded as part of
    /// their parent's recursive lifecycle operations.
    /// </remarks>
    public readonly bool Persistent;

    /// <summary>
    /// Gets the collection of nodes directly contained by this node.
    /// </summary>
    public readonly NodeGroup Subnodes;

    /// <summary>
    /// Gets the tags assigned to this node.
    /// </summary>
    public readonly Group<string> Tags = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="name">
    /// The initial name of the node.
    /// </param>
    /// <param name="persistent">
    /// Whether the node should persist independently of recursive parent
    /// lifecycle operations.
    /// </param>
    /// <param name="parent">
    /// The initial parent of the node, or <see langword="null"/> to create a
    /// root node.
    /// </param>
    /// <param name="subnodes">
    /// The initial subnodes to add to the node.
    /// </param>
    /// <param name="tags">
    /// The initial tags to assign to the node.
    /// </param>
    public Node(
        string name,
        bool persistent = false,
        Node? parent = null,
        IEnumerable<Node>? subnodes = null,
        IEnumerable<string>? tags = null
    )
    {
        Persistent = persistent;

        Name = new Store<string>(name);
        Parent = new Store<Node?>(null);
        Subnodes = new NodeGroup(this);

        Parent.Connect(OnParentChanged);
        Subnodes.OnAdd.Connect(OnSubnodeAdded);
        Subnodes.OnRemove.Connect(OnSubnodeRemoved);

        if (parent is not null)
            Parent.Set(parent);

        foreach (var node in subnodes ?? [])
            Subnodes.Add(node);

        foreach (var tag in tags ?? [])
            Tags.Add(tag);
    }

    /// <summary>
    /// Gets a value indicating whether the node is currently loaded.
    /// </summary>
    /// <remarks>
    /// A node must have a loaded parent before it can be loaded itself.
    /// </remarks>
    public bool Loaded { get; private set; }

    /// <summary>
    /// Gets the hierarchical path of the node.
    /// </summary>
    /// <remarks>
    /// The path is constructed from the names of the node and all of its
    /// ancestors. Root nodes use only their own name.
    /// </remarks>
    public string Path
    {
        get
        {
            var parent = Parent.Get();

            return parent is null ? Name.Get() : $"{parent.Path}/{Name.Get()}";
        }
    }

    private void OnParentChanged(Node? parent)
    {
        var previous = Parent.Previous;

        if (previous is not null && previous.Subnodes.Contains(this))
            previous.Subnodes.Remove(this);

        if (parent is not null && !parent.Subnodes.Contains(this))
            parent.Subnodes.Add(this);
    }

    private void OnSubnodeAdded(Node node)
    {
        if (node.Parent.Get() != this)
            node.Parent.Set(this);
    }

    private void OnSubnodeRemoved(Node node)
    {
        if (node.Parent.Get() == this)
            node.Parent.Set(null);
    }

    /// <inheritdoc />
    protected override void OnDestroy()
    {
        if (Loaded)
            Unload();

        Parent.Set(null);
        Name.Destroy();

        foreach (var node in Subnodes.ToArray())
            node.Destroy();

        Subnodes.Destroy();
        Tags.Destroy();
        Parent.Destroy();
    }

    /// <summary>
    /// Called when the node is loaded.
    /// </summary>
    /// <remarks>
    /// Override this method to perform node-specific initialization that should
    /// occur when the node enters the loaded state.
    /// </remarks>
    protected virtual void OnLoad() { }

    /// <summary>
    /// Called when the node is unloaded.
    /// </summary>
    /// <remarks>
    /// Override this method to perform node-specific cleanup that should occur
    /// when the node leaves the loaded state.
    /// </remarks>
    protected virtual void OnUnload() { }

    /// <summary>
    /// Throws an exception if the node is not currently loaded.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the node is not loaded.
    /// </exception>
    protected void ThrowIfNotLoaded()
    {
        if (Loaded)
            return;

        throw new InvalidOperationException($"{this} is not loaded.");
    }

    /// <summary>
    /// Loads the node and all non-persistent subnodes.
    /// </summary>
    /// <remarks>
    /// The node must not already be loaded. If the node has a parent, that
    /// parent must be loaded first.
    ///
    /// Persistent subnodes are not automatically loaded as part of this
    /// operation.
    ///
    /// <see cref="OnLoad"/> is invoked before the node enters the loaded state.
    /// Subnodes are loaded after their parent.
    /// </remarks>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the node has already been destroyed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the node is already loaded or its parent is not loaded.
    /// </exception>
    public void Load()
    {
        ThrowIfDestroyed();

        if (Loaded)
            throw new InvalidOperationException($"{this} is already loaded");

        var parent = Parent.Get();

        if (parent is not null && !parent.Loaded)
            throw new InvalidOperationException(
                $"{this} cannot be loaded because its parent is not loaded."
            );

        OnLoad();

        Loaded = true;

        foreach (var node in Subnodes)
        {
            if (node.Persistent)
                continue;

            node.Load();
        }
    }

    /// <summary>
    /// Returns a string representation of the node.
    /// </summary>
    /// <returns>
    /// A string containing the node's path and abbreviated unique identifier.
    /// </returns>
    public override string ToString()
    {
        return $"Node \"{Path}\" ({Identifier.ToString()[..8]})";
    }

    /// <summary>
    /// Unloads the node and all non-persistent subnodes.
    /// </summary>
    /// <remarks>
    /// Persistent subnodes remain loaded when their parent is unloaded.
    ///
    /// Subnodes are unloaded before their parent. <see cref="OnUnload"/> is
    /// invoked after all non-persistent subnodes have been unloaded and before
    /// this node leaves the loaded state.
    /// </remarks>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the node has already been destroyed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the node is not currently loaded.
    /// </exception>
    public void Unload()
    {
        ThrowIfDestroyed();

        if (!Loaded)
            throw new InvalidOperationException($"{this} is not loaded");

        foreach (var node in Subnodes)
        {
            if (node.Persistent)
                continue;

            node.Unload();
        }

        OnUnload();

        Loaded = false;
    }
}
