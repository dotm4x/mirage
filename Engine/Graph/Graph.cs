using Mirage.Common;
using Mirage.Common.Collections;

namespace Mirage.Graph;

public class Graph : Module
{
    private readonly ReactiveDictionary<string, Node> _roots = [];
    public readonly IReadOnlyReactiveDictionary<string, Node> Roots;

    public Graph(IEnumerable<Node> roots)
        : base("Graph")
    {
        Roots = _roots;

        foreach (var root in roots)
            _roots.Add(root.Identifier.ToString(), root);

        _roots.OnAdd.Connect(
            (root) =>
            {
                OnRootAdded(root.Value);
            }
        );
    }

    private void OnRootAdded(Node node)
    {
        if (node.Parent.Get() is not null)
            throw new InvalidOperationException(
                $"Cannot add {node} as a graph root because it already has a parent."
            );
    }
}
