using System;

public sealed class CustomLinkedList<T>
{
    private sealed class Node
    {
        public T Value;
        public Node Next;
        public Node(T value) => Value = value;
    }

    private Node _head;
    private int _count;
    public int Count => _count;

    public void ClearList()
    {
        _head = null;
        _count = 0;
    }

    public void AddLast(T item)
    {
        var newNode = new Node(item);
        if (_head == null)
        {
            _head = newNode;
        }

        else
        {
            var current = _head;
            while (current.Next != null)
                current = current.Next;
            current.Next = newNode;
        }

        _count++;
    }

    public T GetAt(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var current = _head;

        for (var i = 0; i < index; i++)
            current = current.Next;
        return current.Value;
    }
}
