using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EpicAkAnimator : AbstractGunAnimator
{
    Sequence fireSequence;
    Sequence reloadSequence;

    [SerializeField] Transform recoilOrigin;
    [SerializeField] Transform bolt;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletFlapThingy;

    [SerializeField] ObjectPool bulletObjectPool;

    bool curReloading;

    void Start()
    {
        reloadSequence = DOTween.Sequence();
        fireSequence = DOTween.Sequence();

        fireSequence
            .Append(recoilOrigin.DOLocalRotate(new Vector3(-7.0f, 0f, 0f), 0.07f)) // Recoil rot
            .Insert(0.0f, recoilOrigin.DOLocalMove(new Vector3(0f, -0.129f, -1.25f), 0.07f)) // Recoil move
            .Insert(0.0f, bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 0.94f), 0.05f)) // Move Bolt
            .Append(recoilOrigin.DOLocalMove(new Vector3(0f, -0.129f, -1.18f), 0.06f)) // Recoil move back
            .Insert(0.15f, recoilOrigin.DOLocalRotate(new Vector3(0.0f, 0f, 0f), 0.2f)) // Recoil rot back
            .Insert(0.15f, bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 1.21f), 0.05f)); // Move Bolt

        reloadSequence
            .Append(bulletFlapThingy.DOLocalRotate(new Vector3(0, 0f, 0f), 0.2f))
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0.0f,0f,55f), 0.2f)) // Rotate to see bolt
            .AppendCallback(() => PlayAudio.Invoke(0))
            .Append(bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 0.94f), 0.2f)) // Move Bolt
            .AppendCallback(() => PlayAudio.Invoke(1))
            .Append(bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 1.21f), 0.2f)) // Move Bolt
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0.0f,0f,0f), 0.2f)) // Rotate back to normal
            .AppendCallback(() => curReloading = false);
    }

    IEnumerator SpawnBullets()
    {
        for (int i = 0; i < 60; i++)
        {
            GameObject bulletInstance = bulletObjectPool.GetObject();
            bulletInstance.GetComponent<Renderer>().material.DOFade(0.0f, 0.0f);
            bulletInstance.transform.localPosition = new Vector3(0, 1.89f, 1.1f);
            bulletInstance.GetComponent<Renderer>().material.DOFade(1.0f, 0.2f);
            yield return new WaitForSeconds(0.05f);
            bulletInstance.transform.DOLocalMove(new Vector3(0f, 0.624f, 1.1f), 0.4f).OnStart(() =>
            {
                PlayAudio.Invoke(2);
            });
            bulletInstance.GetComponent<Renderer>().material.DOFade(0.0f, 0.75f).onComplete += () =>
            {
                addToMag.Invoke(1);
                bulletObjectPool.ReleaseObject(bulletInstance);
            };
        }
    }

    IEnumerator ReloadCoroutine()
    {
        curReloading = true;
        bulletFlapThingy.DOLocalRotate(new Vector3(-70f, 0f, 0f), 0.2f);
        yield return StartCoroutine(SpawnBullets());
        yield return new WaitForSeconds(0.4f);
        reloadSequence.Restart();
    }

    public override void Fire()
    {
        fireSequence.Restart();
    }

    public override void Reload()
    {
        StartCoroutine(ReloadCoroutine());
    }

    public override bool IsReloading()
    {
        return curReloading;
    }
}
