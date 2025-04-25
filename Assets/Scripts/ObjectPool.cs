using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Pool;

public enum ReturnType
{
    Collision,
    Time,
    ParticleSystem,
    Manual
}

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize;
    public int maxPoolSize;

    public ReturnType returnType;

    [ShowIf("returnType", ReturnType.Time)] public float returnTime;

    IObjectPool<GameObject> pool;

    void Start()
    {
        // create object pool
        pool = new ObjectPool<GameObject>(CreateObject, OnTakeFromPool, OnReturnToPool, OnDestroyPoolObject, false, poolSize, maxPoolSize);
    }

    GameObject CreateObject()
    {
        GameObject obj = Instantiate(prefab); // create instance of object

        if (returnType != ReturnType.Manual) 
        {
            ReturnToPool returnToPool = obj.AddComponent<ReturnToPool>(); // add ReturnToPool component to object if neeeded
            // set variables
            returnToPool.pool = pool;
            returnToPool.returnType = returnType;
            returnToPool.time = returnTime;
        }

        return obj;
    }

    void OnTakeFromPool(GameObject obj)
    {
        obj.SetActive(true); // enable object
    }

    void OnReturnToPool(GameObject obj)
    {
        obj.SetActive(false); // disable object
    }

    void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj); // destroy object
    }

    // for special ShowIf attribute
    private bool isTimeBased() {
        return returnType == ReturnType.Time;
    }

    public GameObject GetObject()
    {
        return pool.Get();
    }

    public void ReleaseObject(GameObject obj)
    {
        pool.Release(obj);
    }

    public void DestroyPool()
    {
        pool.Clear();
    }
}

public class ReturnToPool : MonoBehaviour
{
    [HideInInspector] public IObjectPool<GameObject> pool;
    [HideInInspector] public ReturnType returnType;

    [HideInInspector] public float time;

    Collider col;
    ParticleSystem particleSystem;

    void Awake()
    {
        switch (returnType)
        {
            case ReturnType.Collision:
                col = gameObject.GetComponent<Collider>();
                break;
            case ReturnType.ParticleSystem:
                particleSystem = gameObject.GetComponent<ParticleSystem>();
                break;
            case ReturnType.Time:
                Invoke(nameof(ReturnObject), time);
                break;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (returnType == ReturnType.Collision)
        {
            ReturnObject();
        }
    }

    void OnParticleSystemStopped()
    {
        if (returnType == ReturnType.ParticleSystem)
        {
            ReturnObject();
        }        
    }

    void ReturnObject()
    {
        pool.Release(gameObject);
    }
}
