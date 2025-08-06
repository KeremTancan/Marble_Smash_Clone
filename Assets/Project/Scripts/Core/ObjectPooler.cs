using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] private GameObject prefabToPool;
    [SerializeField] private int initialPoolSize = 20;

    private Queue<GameObject> _pool = new Queue<GameObject>();
    private Transform _parent;

    private void Awake()
    {
        _parent = new GameObject(prefabToPool.name + "_Pool").transform;
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(prefabToPool, _parent);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public GameObject GetObjectFromPool()
    {
        if (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        
        // Eğer havuzda obje kalmamışsa, acil durum için yeni bir tane oluştur.
        GameObject newObj = Instantiate(prefabToPool, _parent);
        return newObj;
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }
}