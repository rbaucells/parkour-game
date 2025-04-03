using UnityEngine;

public abstract class AbstractGunAnimator : MonoBehaviour
{
    public abstract void Fire();

    public abstract void Reload();

    public abstract bool IsReloading();
}
