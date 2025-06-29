using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{
    public enum ReturnType
    {
        Collision,
        Time,
        CollisionAndTime,
        ParticleSystem,
        Manual
    }
    public GameObject prefab;
    public GameObject parent;
    public int poolSize;
    public int maxPoolSize;
    public ReturnType returnType;

    [ShowIf(nameof(HasTime))] public float returnTime;

    IObjectPool<GameObject> pool;

    void Start()
    {
        // create object pool
        pool = new ObjectPool<GameObject>(CreateObject, OnTakeFromPool, OnReturnToPool, OnDestroyPoolObject, true, poolSize, maxPoolSize);
    }

    GameObject CreateObject()
    {
        GameObject obj;

        if (parent == null)
        {
            obj = Instantiate(prefab); // create instance of object    
            obj.transform.position = Vector3.zero; // reset position        
            obj.transform.rotation = Quaternion.identity; // reset rotation        
        }
        else
        {
            obj = Instantiate(prefab, parent.transform); // create instance of object with parent
            obj.transform.localPosition = Vector3.zero; // reset position
            obj.transform.localRotation = Quaternion.identity; // reset rotation       
        }

        if (returnType != ReturnType.Manual)
        {
            ReturnToPool returnToPool = obj.AddComponent<ReturnToPool>(); // add ReturnToPool component to object if neeeded
            // set variables
            returnToPool.pool = pool;
            returnToPool.returnType = returnType;
            if (returnType == ReturnType.Time || returnType == ReturnType.CollisionAndTime)
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

    // Helper function for NaughtyAttributes
    public bool HasTime()
    {
        return returnType == ReturnType.Time || returnType == ReturnType.CollisionAndTime;
    }
}