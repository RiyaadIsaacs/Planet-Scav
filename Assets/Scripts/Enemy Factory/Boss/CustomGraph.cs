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

    private GraphNode[] nodes;
    private int nodeCount;
    private int capacity;

    public int NodeCount => nodeCount;

    public CustomGraph(int initialCapacity = 8)
    {
        if (initialCapacity < 1)
            initialCapacity = 8;

        capacity = initialCapacity;
        nodes = new GraphNode[capacity];
        nodeCount = 0;
    }

    public int AddNode(T value)
    {
        if (nodeCount >= capacity)
            ResizeNodes(capacity * 2);

        var index = nodeCount;
        nodes[index] = new GraphNode(value);
        nodeCount++;
        return index;
    }

    public void AddEdge(int from, int to)
    {
        ValidateNodeIndex(from);
        ValidateNodeIndex(to);

        if (from == to)
            return;

        AddNeighborIfMissing(nodes[from], to);
    }

    public T GetNodeValue(int index)
    {
        ValidateNodeIndex(index);
        return nodes[index].Value;
    }

    public int GetNeighborCount(int nodeIndex)
    {
        ValidateNodeIndex(nodeIndex);
        return nodes[nodeIndex].NeighborCount;
    }

    public void CopyNeighbors(int nodeIndex, int[] destination, out int copiedCount)
    {
        ValidateNodeIndex(nodeIndex);
        var node = nodes[nodeIndex];
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
        Array.Copy(nodes, resized, nodeCount);
        nodes = resized;
        capacity = newCapacity;
    }

    private void ValidateNodeIndex(int index)
    {
        if (index < 0 || index >= nodeCount)
            throw new ArgumentOutOfRangeException(nameof(index));
    }
}
