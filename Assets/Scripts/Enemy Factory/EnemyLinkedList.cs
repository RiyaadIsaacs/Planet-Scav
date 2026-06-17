using System;

public sealed class CustomLinkedList<T>
{
    private sealed class Node
    {
        public T Value;
        public Node Next;
        public Node(T value) => Value = value;
    }

    private Node head;
    private int count;
    public int Count => count;

    public void ClearList()
    {
        head = null;
        count = 0;
    }

    public void AddLast(T item)
    {
        var newNode = new Node(item);
        if (head == null)
        {
            head = newNode;
        }

        else
        {
            var current = head;
            while (current.Next != null)
                current = current.Next;
            current.Next = newNode;
        }

        count++;
    }

    public T GetAt(int index)
    {
        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var current = head;

        for (var i = 0; i < index; i++)
            current = current.Next;
        return current.Value;
    }
}
