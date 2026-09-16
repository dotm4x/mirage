using Mirage.Core.Collections;
using Mirage.Core.Events;

namespace Mirage.Graph;

public class Node
{
    public readonly Store<Node?> Parent;
    public readonly Group<Node> Subnodes;

    public Node(Node? parent = null, IEnumerable<Node>? subnodes = null)
    {
        Parent = new Store<Node?>(null);
        Subnodes = [];

        Parent.Connect(OnParentChanged);
        Subnodes.OnAdd.Connect(OnSubnodeAdded);
        Subnodes.OnRemove.Connect(OnSubnodeRemoved);

        if (parent is not null)
        {
            Parent.Set(parent);
        }

        if (subnodes is not null)
        {
            foreach (var node in subnodes)
            {
                Subnodes.Add(node);
            }
        }
    }

    private void OnParentChanged(Node? parent)
    {
        var previous = Parent.Previous;

        if (previous is not null && previous.Subnodes.Contains(this))
        {
            previous.Subnodes.Remove(this);
        }

        if (parent is not null && !parent.Subnodes.Contains(this))
        {
            parent.Subnodes.Add(this);
        }
    }

    private void OnSubnodeAdded(Node node)
    {
        if (node.Parent.Get() != this)
        {
            node.Parent.Set(this);
        }
    }

    private void OnSubnodeRemoved(Node node)
    {
        if (node.Parent.Get() == this)
        {
            node.Parent.Set(null);
        }
    }
}