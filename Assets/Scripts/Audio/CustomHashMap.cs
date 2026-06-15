using System;
using System.Collections.Generic;

// Custom HashMap ADT (separate chaining). [Sealed so it cannot be inherited and it is exclusively used for the SFXManager]
public sealed class CustomHashMap<TKey, TValue>
{
    // A single node in a bucket's chain.
    private sealed class Entry
    {
        public TKey Key;
        public TValue Value;
        public Entry Next;

        public Entry(TKey key, TValue value, Entry next)
        {
            // Store the name ID and the item, and point to the next link in the chain.
            Key = key;
            Value = value;
            Next = next;
        }
    }

    // The row of boxes. Each slot is the head of a chain (linked list).
    private Entry[] buckets;

    // How many key and value pairs we have stored.
    private int count;

    // Public items stored.
    public int Count => count;

    // Create the map with a number of buckets (capacity).
    public CustomHashMap(int capacity = 16)
    {
        if (capacity < 1)
            capacity = 16;

        buckets = new Entry[capacity];
        count = 0;
    }

    // Put a key and value into the map.
    public void Put(TKey key, TValue value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        // Choose which bucket to use for this key.
        int index = GetBucketIndex(key);

        // Walk the chain in that bucket to see if the key is already there.
        Entry current = buckets[index];
        while (current != null)
        {
            // If the same key is found, update the value and exit.
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                current.Value = value;
                return;
            }

            current = current.Next;
        }

        // If key was not found, create a new link and put it at the front of the chain.
        buckets[index] = new Entry(key, value, buckets[index]);
        count++;

        // If we have more items than buckets, make more buckets.
        if (count > buckets.Length)
            Resize(buckets.Length * 2);
    }

    // Try to get a value for a key. Returns true and sets 'value' if found.
    public bool TryGet(TKey key, out TValue value)
    {
        value = default;

        if (key == null)
            return false;

        int index = GetBucketIndex(key);
        Entry current = buckets[index];

        // go down the chain until we find the matching key or reach the end.
        while (current != null)
        {
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                value = current.Value;
                return true;
            }

            current = current.Next;
        }

        return false;
    }

    // Get the value for a key.
    public TValue Get(TKey key)
    {
        if (TryGet(key, out TValue value))
            return value;

        throw new KeyNotFoundException($"Key not found: {key}");
    }

    // Return whether a key exists in the map.
    public bool ContainsKey(TKey key)
    {
        return TryGet(key, out _);
    }

    // Go up the chain keeping track of the previous link so we can unlink the found node.
    public bool Remove(TKey key)
    {
        if (key == null)
            return false;

        int index = GetBucketIndex(key);
        Entry current = buckets[index];
        Entry previous = null;

        while (current != null)
        {
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                // If no previous, remove the first link in the chain.
                if (previous == null)
                    buckets[index] = current.Next;
                else
                    previous.Next = current.Next;

                count--;
                return true;
            }

            previous = current;
            current = current.Next;
        }

        return false;
    }

    // Empty all boxes and reset the count.
    public void Clear()
    {
        for (int i = 0; i < buckets.Length; i++)
            buckets[i] = null;

        count = 0;
    }

    // Convert a key into a bucket index.
    private int GetBucketIndex(TKey key)
    {
        int hash = key.GetHashCode() & 0x7FFFFFFF;
        return hash % buckets.Length;
    }

    // We Create a new buckets array and re-insert all existing entries.
    private void Resize(int newCapacity)
    {
        var oldBuckets = buckets;
        buckets = new Entry[newCapacity];
        count = 0;

        // For each old box, go down the chain and Put each old entry into the new boxes
        for (int i = 0; i < oldBuckets.Length; i++)
        {
            Entry current = oldBuckets[i];
            while (current != null)
            {
                Put(current.Key, current.Value);
                current = current.Next;
            }
        }
    }
}
