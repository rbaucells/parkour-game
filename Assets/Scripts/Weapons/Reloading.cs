using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reloading : MonoBehaviour
{
    [SerializeField] int maxMagSize;
    public int curMag;

    [HideInInspector] public bool reloading {private set; get;}

    void Awake()
    {
        curMag = maxMagSize;
    }
    
    public void Reload()
    {
        curMag = maxMagSize;
    }
}
