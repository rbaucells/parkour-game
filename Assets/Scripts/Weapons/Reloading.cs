using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reloading : MonoBehaviour
{
    public int maxMagSize;
    public int curMag;

    [HideInInspector] public bool reloading {private set; get;}

    AbstractGunAnimator gunAnimator;

    void Start()
    {
        gunAnimator = GetComponent<AbstractGunAnimator>();
        curMag = maxMagSize;
    }

    public void Reload()
    {
        gunAnimator.Reload();
    }
}
