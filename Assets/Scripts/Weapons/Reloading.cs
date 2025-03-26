using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reloading : MonoBehaviour
{
    public int maxMagSize;
    public int curMag;

    [HideInInspector] public bool reloading {private set; get;}

    AbstractGunAnimator gunAnimator;

    void Awake()
    {
        curMag = maxMagSize;
    }

    void Start()
    {
        gunAnimator = GetComponent<AbstractGunAnimator>();
    }

    public void Reload()
    {
        gunAnimator.Reload();
    }
}
