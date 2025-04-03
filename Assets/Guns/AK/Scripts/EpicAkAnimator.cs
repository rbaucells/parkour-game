using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EpicAkAnimator : AbstractGunAnimator
{
    Audio audioPlayer;
    Reloading reloading;
    Sequence fireSequence;
    Sequence reloadSequence;

    [SerializeField] Transform recoilOrigin;
    [SerializeField] Transform bolt;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletFlapThingy;

    void Start()
    {
        audioPlayer = GetComponent<Audio>();
        reloading = GetComponent<Reloading>();

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
            .AppendCallback(() => StartCoroutine(SpawnBullets())) // Spawn bullets
            .Append(bulletFlapThingy.DOLocalRotate(new Vector3(-70f, 0f, 0f), 0.2f))
            .AppendInterval(0.057f * reloading.maxMagSize)
            .Append(bulletFlapThingy.DOLocalRotate(new Vector3(0, 0f, 0f), 0.2f))
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0.0f,0f,55f), 0.2f)) // Rotate to see bolt
            .AppendCallback(() => audioPlayer.PlaySound1())
            .Append(bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 0.94f), 0.2f)) // Move Bolt
            .AppendCallback(() => audioPlayer.PlaySound2())
            .Append(bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 1.21f), 0.2f)) // Move Bolt
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0.0f,0f,0f), 0.2f)); // Rotate back to normal

    }

    IEnumerator SpawnBullets()
    {
        for (int i = 0; i < reloading.maxMagSize; i++)
        {
            GameObject bulletInstance = Instantiate(bullet, recoilOrigin);
            bulletInstance.GetComponent<Renderer>().material.DOFade(0.0f, 0.0f);
            bulletInstance.GetComponent<Renderer>().material.DOFade(1.0f, 0.2f);
            yield return new WaitForSeconds(0.05f);
            bulletInstance.transform.DOLocalMove(new Vector3(0f, 0.624f, 1.1f), 0.4f).OnStart(() =>
            {
                audioPlayer.PlaySound3();
            });
            bulletInstance.GetComponent<Renderer>().material.DOFade(0.0f, 0.75f).onComplete += () =>
            {
                reloading.curMag++;
                Destroy(bulletInstance);
            };
        }
    }

    public override void Fire()
    {
        fireSequence.Restart();
    }

    public override void Reload()
    {
        reloadSequence.Restart();
    }

    public override bool IsReloading()
    {
        return reloadSequence.IsPlaying();
    }
}
