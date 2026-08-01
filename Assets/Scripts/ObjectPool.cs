using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private Queue<GameObject> objectPool;
    [SerializeField] private int poolSize;
    [SerializeField] private GameObject objectReference;
    [SerializeField] private Transform parent;
    [SerializeField] private bool initializeOnStart = true;

    private void Start()
    {
        objectPool = new Queue<GameObject>();

        if (initializeOnStart)
        {
            InitializePool();
        }
    }

    public void InitializePool()
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
        Debug.Log("spawning!");
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

    public void DespawnAllObjects()
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                DespawnObject(transform.GetChild(i).gameObject);
            }
        }
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
        var obj = Instantiate(objectReference, parent);
        objectPool.Enqueue(obj);
        obj.SetActive(false);
    }

    public GameObject SearchPoolByIndex(int index)
    {
        return transform.GetChild(index).gameObject;
    }
}
