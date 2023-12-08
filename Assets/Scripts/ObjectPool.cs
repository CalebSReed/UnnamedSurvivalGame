using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private Queue<GameObject> objectPool;
    [SerializeField] private int poolSize;
    [SerializeField] private GameObject objectReference;
    [SerializeField] private Transform parent;

    private void Awake()
    {
        objectPool = new Queue<GameObject>();
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(objectReference, parent);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
    }


    public GameObject SpawnObject()
    {
        if (IsPoolAtMaxSize())//dynamic pool size just in case we hit this limit for god knows whatever reason
        {
            IncreasePoolSize();
        }

        var obj = objectPool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void DespawnObject(GameObject obj)
    {
        objectPool.Enqueue(obj);
        obj.SetActive(false);
    }

    private bool IsPoolAtMaxSize()
    {
        if (objectPool.Count == 0)
        {
            return true;
        }
        return false;
    }

    private void IncreasePoolSize()
    {
        poolSize++;
        var obj = Instantiate(objectReference);
        objectPool.Enqueue(obj);
        obj.SetActive(false);
    }
}
