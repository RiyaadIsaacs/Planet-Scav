using UnityEngine;

public class DialogueQueue : MonoBehaviour
{
    // Nested class node, single item of the queue
    class Node
    {
        public DialogueItem data; // stores the dialogue and info 
        public Node next; // pointer to the next node in the list

        // Constructor to initialize the node with a dialogue item
        public Node(DialogueItem item)
        {
            data = item;
            next = null;
        }
    }

    private Node front; // Pointer to the front of the queue
    private Node rear; // Pointer to the rear of the queue

    // Add a dialogue item to the rear of the queue
    public void Enqueue(DialogueItem item)
    {
        Node newNode = new Node(item);

        // If queue is empty, both front and rear point to the new node 
        if (rear == null)
        {
            front = rear = newNode;
            return;
        }

        rear.next = newNode;
        rear = newNode;
    }

    // Remove and return the dialogue item at the front of the queue
    public DialogueItem Dequeue()
    {
        // If queue is empty, return null
        if (front == null)
            return null;

        // Store the current front node's item and move the front pointer to the next node
        DialogueItem item = front.data;
        front = front.next;

        // If queue is empty after dequeuing, set rear to null
        if (front == null)
            rear = null;

        return item;
    }

    // Check if the queue is empty
    public bool IsEmpty()
    {
        return front == null;
    }

}   
