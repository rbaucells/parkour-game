using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{
    public enum ReturnType
    {
        Collision,
        Time,
        ParticleSystem,
        Manual
    }
    public GameObject prefab;
    public GameObject parent;
    public int poolSize;
    public int maxPoolSize;
    public ReturnType returnType;

    [ShowIf("returnType", ReturnType.Time)] public float returnTime;

    IObjectPool<GameObject> pool;

    void Start()
    {
        // create object pool
        pool = new ObjectPool<GameObject>(CreateObject, OnTakeFromPool, OnReturnToPool, OnDestroyPoolObject, true, poolSize, maxPoolSize);
    }

    GameObject CreateObject()
    {
        // Debug.Log("Create Object");
        GameObject obj;

        if (parent == null)
        {
            obj = Instantiate(prefab); // create instance of object    
            obj.transform.position = Vector3.zero; // reset position        
        }
        else
        {
            obj = Instantiate(prefab, parent.transform); // create instance of object with parent
            obj.transform.localPosition = Vector3.zero; // reset position
        }

        if (returnType != ReturnType.Manual) 
        {
            ReturnToPool returnToPool = obj.AddComponent<ReturnToPool>(); // add ReturnToPool component to object if neeeded
            // set variables
            returnToPool.pool = pool;
            returnToPool.returnType = returnType;
            if (returnType == ReturnType.Time)
                returnToPool.time = returnTime;
        }

        return obj;
    }

    void OnTakeFromPool(GameObject obj)
    {
        // Debug.Log("Get Object");
        obj.SetActive(true); // enable object
    }

    void OnReturnToPool(GameObject obj)
    {
        // Debug.Log("Return Object");
        obj.SetActive(false); // disable object
    }

    void OnDestroyPoolObject(GameObject obj)
    {
        // Debug.Log("Destroy Object");
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
}