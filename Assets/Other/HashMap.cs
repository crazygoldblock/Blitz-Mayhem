using System.Collections;
using System.Collections.Generic;

public class HashMap<K, V>
{
    private readonly Dictionary<K, V> map = new();

    public int Count
    {
        get { return map.Count; }
    }

    public bool Add(K key, V value)
    {
        try
        {
            map.Add(key, value);
        }
        catch
        {
            return false;
        }
        return true;
    }
    public bool Remove(K key)
    {
        return map.Remove(key);
    }
    public V Get(K key)
    {
        map.TryGetValue(key, out V temp);
        return temp;
    }

    public IEnumerator GetEnumerator()
    {
        return map.GetEnumerator();
    }
}
public class LinearList<T>
{
    private readonly LinkedList<T> queue = new();
    
    public void AddLast(T item)
    {
        queue.AddLast(item);
    }
    public void RemoveFirst()
    {
        queue.RemoveFirst();
    }
    public int Count
    {
        get
        {
            return queue.Count;
        }
    }
    public T this[int index]
    {
        get
        {
            int i = 0; ;
            foreach (T item in queue)
            {
                if (i == index)
                {
                    return item;
                }
                i++;
            }
            return default;
        }
    }
}
