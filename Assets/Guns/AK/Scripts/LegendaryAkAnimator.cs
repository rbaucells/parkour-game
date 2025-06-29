using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LegendaryAkAnimator : AbstractGunAnimator
{
    Sequence fireSequence;
    Sequence reloadSequence;

    [SerializeField] Transform recoilOrigin;
    [SerializeField] Transform bolt;
    [SerializeField] GameObject bullet;

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
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0.0f,0f,55f), 0.2f)) // Rotate to see bolt
            .AppendCallback(() => PlayAudio.Invoke(0))
            .Append(bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 0.94f), 0.2f)) // Move Bolt
            .AppendCallback(() => PlayAudio.Invoke(1))
            .Append(bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 1.21f), 0.2f)) // Move Bolt
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0.0f,0f,0f), 0.2f)) // Rotate back to normal
            .AppendCallback(() => curReloading = false);
    }

    IEnumerator reloadCoroutine()
    {
        curReloading = true;
        yield return StartCoroutine(SpawnBullets());
        yield return new WaitForSeconds(0.3f);
        reloadSequence.Restart();
    }

    IEnumerator SpawnBullets()
    {
        for (int i = 0; i < 200; i++)
        {
            GameObject bulletInstance = bulletObjectPool.GetObject();
            bulletInstance.GetComponent<Renderer>().material.DOFade(0.0f, 0.0f);
            bulletInstance.transform.localPosition = new Vector3(0f, 0.94f, 0.471f);
            bulletInstance.GetComponent<Renderer>().material.DOFade(1.0f, 0.3f);
            yield return new WaitForSeconds(0.1f);
            bulletInstance.transform.DOLocalMove(new Vector3(0, 0.94f, 1.286f), 0.3f).OnComplete(() =>
            {
                PlayAudio.Invoke(2);
            });
            bulletInstance.GetComponent<Renderer>().material.DOFade(0.0f, 0.5f).onComplete += () =>
            {
                addToMag.Invoke(1);
                bulletObjectPool.ReleaseObject(bulletInstance);
            };
        }
    }

    public override void Fire()
    {
        fireSequence.Restart();
    }

    public override void Reload()
    {
        StartCoroutine(reloadCoroutine()); 
    }

    public override bool IsReloading()
    {
        return curReloading;
    }
}
