using Mirage.Graph;

var node = new Node(name: "Root");

var myNode = new MyNode(name: "MyNode", parent: node, subnodes: [new MyNode("MySubnode")]);

Console.WriteLine(node.Subnodes.GetByPath("MyNode/MySubnode")!.Path);

internal class MyNode(
    string name = "MyNode",
    Node? parent = null,
    IEnumerable<Node>? subnodes = null
) : Node(name: name, parent: parent, subnodes: subnodes)
{
    protected override void OnLoad()
    {
        Console.WriteLine($"Node {Identifier} loaded");
    }

    public void Greet()
    {
        ThrowIfNotLoaded();

        Console.WriteLine("Hello World!");
    }
}
