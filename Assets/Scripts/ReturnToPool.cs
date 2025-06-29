using UnityEngine;
using UnityEngine.Pool;

public class ReturnToPool : MonoBehaviour
{
    [HideInInspector] public IObjectPool<GameObject> pool;
    [HideInInspector] public ObjectPool.ReturnType returnType;
    [HideInInspector] public float time;

    void OnEnable()
    {
        if (returnType == ObjectPool.ReturnType.Time || returnType == ObjectPool.ReturnType.CollisionAndTime)
            Invoke(nameof(ReturnObject), time);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(ReturnObject));
    }

    void OnCollisionEnter(Collision other)
    {
        if (returnType == ObjectPool.ReturnType.Collision || returnType == ObjectPool.ReturnType.CollisionAndTime)
            ReturnObject();
    }

    void OnParticleSystemStopped()
    {
        if (returnType == ObjectPool.ReturnType.ParticleSystem)
            ReturnObject();
    }

    void ReturnObject()
    {
        if (gameObject.activeInHierarchy == true)
            pool.Release(gameObject);
    }
}

