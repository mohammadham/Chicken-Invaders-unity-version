using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for performance optimization
/// </summary>
public class ObjectPool<T> where T : Component
{
    private Queue<T> pool = new Queue<T>();
    private T prefab;
    private Transform parent;
    
    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        
        // Pre-populate pool
        for (int i = 0; i < initialSize; i++)
        {
            T obj = CreateObject();
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    
    T CreateObject()
    {
        T obj = Object.Instantiate(prefab, parent);
        return obj;
    }
    
    public T Get()
    {
        if (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            // Pool is empty, create new object
            return CreateObject();
        }
    }
    
    public void Return(T obj)
    {
        if (obj != null)
        {
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    
    public void Clear()
    {
        while (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            if (obj != null)
                Object.Destroy(obj.gameObject);
        }
    }
}