using UnityEngine;
using UnityEngine.Pool;

public class ReturnToPool : MonoBehaviour
{
    [HideInInspector] public IObjectPool<GameObject> pool;
    [HideInInspector] public ObjectPool.ReturnType returnType;
    [HideInInspector] public float time;

    void Awake()
    {
        if (returnType == ObjectPool.ReturnType.Time)
            Invoke(nameof(ReturnObject), time);
    }

    void OnCollisionEnter(Collision other)
    {
        if (returnType == ObjectPool.ReturnType.Collision)
            ReturnObject();
    }

    void OnParticleSystemStopped()
    {
        if (returnType == ObjectPool.ReturnType.ParticleSystem)
            ReturnObject();      
    }

    void ReturnObject()
    {
        Debug.Log("Return to Pool");
        pool.Release(gameObject);
    }
}

