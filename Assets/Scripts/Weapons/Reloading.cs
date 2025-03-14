using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reloading : MonoBehaviour
{
    public enum ReloadType
    {
        Magazine,
        Repeating
    }
    [SerializeField] ReloadType reloadType;
    [SerializeField] int maxMagSize;
    public int curMag;

    [HideInInspector] public bool reloading {private set; get;}

    void Awake()
    {
        curMag = maxMagSize;
    }
    
    public void Reload()
    {
        switch (reloadType)
        {
            case ReloadType.Magazine:
                MagazineReload();
                break;
            case ReloadType.Repeating:
                break;
        }
    }

    void MagazineReload()
    {
        curMag = maxMagSize;
    }
}
