using System;
using System.Collections.Generic;

[Serializable]
public class PriorityQueue<Node> where Node : IComparable<Node>
{
    private List<Node> _heap = new List<Node>();

    public int Count => _heap.Count;
    public bool Any() => _heap.Count > 0;

    public void Enqueue(Node item)
    {
        _heap.Add(item);
        HeapifyUp(_heap.Count - 1);
    }

    public Node Dequeue()
    {
        if (_heap.Count == 0) return default;
        Node root = _heap[0];
        int lastIndex = _heap.Count - 1;
        _heap[0] = _heap[lastIndex];
        _heap.RemoveAt(lastIndex);
        HeapifyDown(0);
        return root;
    }

    public void RemoveAll(Predicate<Node> match)
    {
        _heap.RemoveAll(match);
        BuildHeap();
    }

    private void BuildHeap()
    {
        for (int i = (_heap.Count / 2) - 1; i >= 0; i--)
        {
            HeapifyDown(i);
        }
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (_heap[parent].CompareTo(_heap[index]) <= 0) break;
            Swap(parent, index);
            index = parent;
        }
    }

    private void HeapifyDown(int index)
    {
        int lastIndex = _heap.Count - 1;
        while (true)
        {
            int left = 2 * index + 1, right = 2 * index + 2, smallest = index;
            if (left <= lastIndex && _heap[left].CompareTo(_heap[smallest]) < 0) smallest = left;
            if (right <= lastIndex && _heap[right].CompareTo(_heap[smallest]) < 0) smallest = right;
            if (smallest == index) break;
            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
    }

    public void Clear()
    {
        _heap.Clear();
    }
}
