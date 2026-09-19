using Mirage.Graph;

namespace Tests.Graph;

public class NodeTest
{
    [Fact]
    public void Constructor_WithParent_SynchronizesParentAndSubnodes()
    {
        var parent = new Node("Parent");
        var child = new Node("Child", parent: parent);

        Assert.Same(parent, child.Parent.Get());
        Assert.Contains(child, parent.Subnodes);
        Assert.Equal("Parent/Child", child.Path);
    }

    [Fact]
    public void Parent_WhenChanged_SynchronizesBothHierarchies()
    {
        var firstParent = new Node("First");
        var secondParent = new Node("Second");
        var child = new Node("Child", parent: firstParent);

        child.Parent.Set(secondParent);

        Assert.DoesNotContain(child, firstParent.Subnodes);
        Assert.Contains(child, secondParent.Subnodes);
        Assert.Same(secondParent, child.Parent.Get());
    }

    [Fact]
    public void Load_ComposesBeforeOnLoad_AndOnlyOnce()
    {
        var composed = new Node("Composed");
        var node = new TrackingNode("Node", [composed]);

        node.Load();
        node.Unload();
        node.Load();

        Assert.Equal(1, node.ComposeCalls);
        Assert.Equal(2, node.LoadCalls);
        Assert.Same(composed, node.Subnodes.Single());
        Assert.True(node.SawComposedNodeDuringLoad);
    }

    [Fact]
    public void Load_LoadsNonPersistentSubnodesButNotPersistentSubnodes()
    {
        var regular = new Node("Regular");
        var persistent = new Node("Persistent", persistent: true);
        var parent = new Node("Parent", subnodes: [regular, persistent]);

        parent.Load();

        Assert.True(parent.Loaded);
        Assert.True(regular.Loaded);
        Assert.False(persistent.Loaded);
    }

    [Fact]
    public void Unload_LeavesPersistentSubnodesLoaded()
    {
        var parent = new Node("Parent");
        var persistent = new Node("Persistent", persistent: true, parent: parent);

        parent.Load();
        persistent.Load();
        parent.Unload();

        Assert.False(parent.Loaded);
        Assert.True(persistent.Loaded);
    }

    [Fact]
    public void Subnodes_GetByPath_ResolvesCurrentParentAndDescendants()
    {
        var root = new Node("Root");
        var child = new Node("Child", parent: root);
        var grandchild = new Node("Grandchild", parent: child);

        Assert.Same(root, root.Subnodes.GetByPath("."));
        Assert.Same(child, root.Subnodes.GetByPath("Child"));
        Assert.Same(grandchild, root.Subnodes.GetByPath("Child/Grandchild"));
        Assert.Same(root, grandchild.Subnodes.GetByPath("../.."));
    }

    [Fact]
    public void Subnodes_GetByTagRecursive_FindsDescendants()
    {
        var direct = new Node("Direct", tags: ["target"]);
        var nested = new Node("Nested", tags: ["target"]);
        var parent = new Node("Parent", subnodes: [direct, new Node("Branch", subnodes: [nested])]);

        Assert.Equal([direct], parent.Subnodes.GetByTag("target").ToArray());
        Assert.Equal([direct, nested], parent.Subnodes.GetByTag("target", true).ToArray());
    }

    private sealed class TrackingNode(string name, IEnumerable<Node> composed) : Node(name)
    {
        private readonly IEnumerable<Node> _composed = composed;

        public int ComposeCalls { get; private set; }

        public int LoadCalls { get; private set; }

        public bool SawComposedNodeDuringLoad { get; private set; }

        protected override IEnumerable<Node> Compose()
        {
            ComposeCalls++;
            return _composed;
        }

        protected override void OnLoad()
        {
            LoadCalls++;
            SawComposedNodeDuringLoad = Subnodes.Count == _composed.Count();
        }
    }
}
