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

    private void Awake()
    {
        objectPool = new Queue<GameObject>();

        if (initializeOnStart)
        {
            InitializePool();
        }
    }

    public void InitializePool()
    {
        Debug.Log("initializing pool");
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(objectReference, parent);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
    }


    public GameObject SpawnObject()
    {
        if (IsPoolAtMaxSize())
        {
            IncreasePoolSize();//make room for another object
        }

        var obj = objectPool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void DespawnObject(GameObject obj)
    {
        obj.SetActive(false);
        objectPool.Enqueue(obj);
    }

    public void DespawnAllObjects()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        objectPool.Clear();
        InitializePool();
    }

    private bool IsPoolAtMaxSize()
    {
        if (objectPool.Count == 0)//we have used everything in the queue
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
