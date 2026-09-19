using Mirage.Common;
using Mirage.Common.Lifecycle;
using Mirage.Graph;

namespace Tests.GraphTests;

public class GraphTest
{
    [Fact]
    public void Constructor_RegistersInitialAndAdditionalRoots()
    {
        var initial = new Node("Initial");
        var additional = new Node("Additional");

        var graph = new Mirage.Graph.Graph(initial, [additional]);

        Assert.Equal(2, graph.Roots.Count);
        Assert.Same(initial, graph.Roots["Initial"]);
        Assert.Same(additional, graph.Roots["Additional"]);
        Assert.Same(initial, graph.ActiveRoot.Get());
    }

    [Fact]
    public void Start_LoadsActiveRoot()
    {
        var root = new Node("Root");
        var graph = new Mirage.Graph.Graph(root);

        var game = new TestGame(graph);
        game.Start();

        Assert.Equal(ModuleState.Running, graph.State.Get());
        Assert.True(root.Loaded);

        game.Stop();
    }

    [Fact]
    public void Switch_WhenIdle_ChangesActiveRootWithoutLoadingIt()
    {
        var initial = new Node("Initial");
        var next = new Node("Next");
        var graph = new Mirage.Graph.Graph(initial, [next]);

        graph.Switch("Next");

        Assert.Same(next, graph.ActiveRoot.Get());
        Assert.False(initial.Loaded);
        Assert.False(next.Loaded);
    }

    [Fact]
    public void Switch_WhenRunning_UnloadsCurrentRootAndLoadsNextRoot()
    {
        var initial = new Node("Initial");
        var next = new Node("Next");
        var graph = new Mirage.Graph.Graph(initial, [next]);

        var game = new TestGame(graph);
        game.Start();
        graph.Switch("Next");

        Assert.Same(next, graph.ActiveRoot.Get());
        Assert.False(initial.Loaded);
        Assert.True(next.Loaded);

        game.Stop();
    }

    [Fact]
    public void Switch_WhenRootDoesNotExist_Throws()
    {
        var graph = new Mirage.Graph.Graph(new Node("Initial"));

        Assert.Throws<InvalidOperationException>(() => graph.Switch("Missing"));
    }

    [Fact]
    public void Roots_WhenRemovingActiveRoot_RestoresRootAndThrows()
    {
        var root = new Node("Root");
        var graph = new Mirage.Graph.Graph(root);

        Assert.Throws<InvalidOperationException>(() => graph.Roots.Remove("Root"));

        Assert.Same(root, graph.Roots["Root"]);
        Assert.Same(root, graph.ActiveRoot.Get());
    }

    [Fact]
    public void Roots_WhenRegisteringChild_Throws()
    {
        var root = new Node("Root");
        var child = new Node("Child", parent: root);
        var graph = new Mirage.Graph.Graph(root);

        Assert.Throws<InvalidOperationException>(() => graph.Roots.Add("Child", child));
        Assert.DoesNotContain(graph.Roots, entry => ReferenceEquals(entry.Value, child));
    }

    private sealed class TestGame(Module module) : Game([module]);
}
