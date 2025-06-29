using UnityEngine;
using UnityEngine.Events;

public abstract class AbstractGunAnimator : MonoBehaviour
{
    [HideInInspector] public UnityEvent<int> PlayAudio = new UnityEvent<int>();
    [HideInInspector] public UnityEvent onReload = new UnityEvent();
    [HideInInspector] public UnityEvent<int> addToMag = new UnityEvent<int>();

    public abstract void Fire();

    public abstract void Reload();

    public abstract bool IsReloading();
}
