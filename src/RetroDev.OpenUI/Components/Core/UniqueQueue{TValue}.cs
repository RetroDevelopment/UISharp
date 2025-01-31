namespace RetroDev.OpenUI.Components.Core;

internal class UniqueQueue<TValue>
{
    private readonly Queue<TValue> _queue = [];
    private readonly HashSet<TValue> _set = [];

    public int Count => _queue.Count;
    public bool Empty => Count == 0;

    public void Enqueue(TValue item)
    {
        if (_set.Add(item)) // Only add if it's not already in the set
        {
            _queue.Enqueue(item);
        }
    }

    public TValue Dequeue()
    {
        if (_queue.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        TValue item = _queue.Dequeue();
        _set.Remove(item); // Remove from set as well
        return item;
    }

    public TValue Peek() => _queue.Peek();

    public bool Contains(TValue item) => _set.Contains(item);

    public void Clear()
    {
        _queue.Clear();
        _set.Clear();
    }
}
