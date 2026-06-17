public class DialogueQueue
{
    class Node
    {
        public DialogueItem data;
        public Node next;

        public Node(DialogueItem item)
        {
            data = item;
            next = null;
        }
    }

    private Node front;
    private Node rear;

    public void Enqueue(DialogueItem item)
    {
        Node newNode = new Node(item);

        if (rear == null)
        {
            front = rear = newNode;
            return;
        }

        rear.next = newNode;
        rear = newNode;
    }

    public DialogueItem Dequeue()
    {
        if (front == null)
            return null;

        DialogueItem item = front.data;
        front = front.next;

        if (front == null)
            rear = null;

        return item;
    }

    public bool IsEmpty()
    {
        return front == null;
    }
}
