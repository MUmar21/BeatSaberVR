using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform poolParent;
    private readonly bool autoExpand;
    private readonly Queue<T> pool = new();
    private readonly Action<T> onGet;
    private readonly Action<T> onReturn;

    public ObjectPool(T prefab, int initialCount = 5, Transform poolParent = null, bool autoExpand = true,
                          Action<T> onGet = null, Action<T> onReturn = null)
    {
        this.prefab = prefab;
        this.poolParent = poolParent;
        this.autoExpand = autoExpand;
        this.onGet = onGet;
        this.onReturn = onReturn;

        for (int i = 0; i < initialCount; i++)
            CreateNew();
    }

    protected T CreateNew()
    {
        var obj = UnityEngine.Object.Instantiate(prefab, poolParent);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public virtual T Get()
    {
        if (pool.Count == 0 && autoExpand)
            CreateNew();

        if (pool.Count == 0)
        {
            Debug.LogWarning("Pool exhausted and auto-expand disabled!");
            return null;
        }

        var pooledObj = pool.Dequeue();
        onGet?.Invoke(pooledObj);
        return pooledObj;
    }

    public virtual void ReturnToPool(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        onReturn?.Invoke(obj);
    }
}
