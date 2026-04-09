using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public GameObject Get(string poolKey)
    {
        if (_pools.TryGetValue(poolKey, out var queue) && queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null; // caller must create new
    }

    public void Return(string poolKey, GameObject obj)
    {
        obj.SetActive(false);

        if (!_pools.ContainsKey(poolKey))
        {
            _pools[poolKey] = new Queue<GameObject>();
        }
        _pools[poolKey].Enqueue(obj);
    }

    public void ClearAll()
    {
        foreach (var kvp in _pools)
        {
            foreach (var obj in kvp.Value)
            {
                if (obj != null) Destroy(obj);
            }
        }
        _pools.Clear();
    }
}
