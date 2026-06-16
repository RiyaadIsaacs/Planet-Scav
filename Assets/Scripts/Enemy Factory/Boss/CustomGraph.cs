using System;

public sealed class CustomGraph<T>
{
    private sealed class GraphNode
    {
        public T Value;
        public int[] Neighbors;
        public int NeighborCount;

        public GraphNode(T value)
        {
            Value = value;
            Neighbors = new int[4];
            NeighborCount = 0;
        }
    }

    private GraphNode[] _nodes;
    private int _nodeCount;
    private int _capacity;

    public int NodeCount => _nodeCount;

    public CustomGraph(int initialCapacity = 8)
    {
        if (initialCapacity < 1)
            initialCapacity = 8;

        _capacity = initialCapacity;
        _nodes = new GraphNode[_capacity];
        _nodeCount = 0;
    }

    public int AddNode(T value)
    {
        if (_nodeCount >= _capacity)
            ResizeNodes(_capacity * 2);

        var index = _nodeCount;
        _nodes[index] = new GraphNode(value);
        _nodeCount++;
        return index;
    }

    public void AddEdge(int from, int to)
    {
        ValidateNodeIndex(from);
        ValidateNodeIndex(to);

        if (from == to)
            return;

        AddNeighborIfMissing(_nodes[from], to);
    }

    public T GetNodeValue(int index)
    {
        ValidateNodeIndex(index);
        return _nodes[index].Value;
    }

    public int GetNeighborCount(int nodeIndex)
    {
        ValidateNodeIndex(nodeIndex);
        return _nodes[nodeIndex].NeighborCount;
    }

    public void CopyNeighbors(int nodeIndex, int[] destination, out int copiedCount)
    {
        ValidateNodeIndex(nodeIndex);
        var node = _nodes[nodeIndex];
        copiedCount = node.NeighborCount;

        if (destination == null || destination.Length < copiedCount)
            throw new ArgumentException("Destination array is too small.", nameof(destination));

        for (var i = 0; i < copiedCount; i++)
            destination[i] = node.Neighbors[i];
    }

    private static void AddNeighborIfMissing(GraphNode node, int neighborIndex)
    {
        for (var i = 0; i < node.NeighborCount; i++)
        {
            if (node.Neighbors[i] == neighborIndex)
                return;
        }

        if (node.NeighborCount >= node.Neighbors.Length)
            Array.Resize(ref node.Neighbors, node.Neighbors.Length * 2);

        node.Neighbors[node.NeighborCount] = neighborIndex;
        node.NeighborCount++;
    }

    private void ResizeNodes(int newCapacity)
    {
        var resized = new GraphNode[newCapacity];
        Array.Copy(_nodes, resized, _nodeCount);
        _nodes = resized;
        _capacity = newCapacity;
    }

    private void ValidateNodeIndex(int index)
    {
        if (index < 0 || index >= _nodeCount)
            throw new ArgumentOutOfRangeException(nameof(index));
    }
}
