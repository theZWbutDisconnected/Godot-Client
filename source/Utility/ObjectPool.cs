namespace TestClient.Source.Utility;

using System;
using System.Collections.Concurrent;


public class ObjectPool<T> where T : new()
{
    private readonly ConcurrentBag<T> _items = new();
    private readonly Func<T> _generator;
    private readonly int _capacity;

    public ObjectPool(Func<T> generator, int capacity)
    {
        _generator = generator;
        _capacity = capacity;
    }

    public T Get()
    {
        return _items.TryTake(out var item) ? item : _generator();
    }

    public void Return(T item)
    {
        if (_items.Count < _capacity)
            _items.Add(item);
        else if (item is IDisposable disposable)
            disposable.Dispose();
    }
}