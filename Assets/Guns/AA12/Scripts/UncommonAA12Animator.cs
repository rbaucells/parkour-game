using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UncommonAA12Animator : AbstractGunAnimator
{
    Audio audioPlayer;
    Reloading reloading;
    Sequence reloadSequence;
    Sequence fireSequence;

    [SerializeField] Transform recoilOrigin;
    [SerializeField] Transform magazineOrigin;
    [SerializeField] Transform bolt;
    void Start()
    {
        audioPlayer = GetComponent<Audio>();
        reloading = GetComponent<Reloading>();

        fireSequence = DOTween.Sequence();
        reloadSequence = DOTween.Sequence();

        fireSequence
            .Append(recoilOrigin.DOLocalRotate(new Vector3(-10f, 0f, 0.0f), 0.07f))
            .Join(recoilOrigin.DOLocalMove(new Vector3(0.0f,-0.5f, -1.4f), 0.05f))
            .Join(bolt.DOLocalMove(new Vector3(0.03f, 0.53f, 1.1f), 0.03f))
            .Append(bolt.DOLocalMove(new Vector3(0.03f, 0.53f, 1.418f), 0.03f))
            .Join(recoilOrigin.DOLocalRotate(new Vector3(0.0f, 0.0f, 0.0f), 0.25f))
            .Join(recoilOrigin.DOLocalMove(new Vector3(0f, -0.358f, -1.368f), 0.2f));

        reloadSequence
            .AppendCallback(() =>
            {
                magazineOrigin.localPosition = new Vector3(0, 0.384f, 1.101f); // Set Starting Pos
                magazineOrigin.localRotation = Quaternion.Euler(-3.55f, 0, 0); // Set Starting Rot
            })
            .AppendCallback(() => audioPlayer.PlaySound1())
            .Append(magazineOrigin.GetComponentInChildren<Renderer>().material.DOFade(0.0f, 0.3f)) // Fade into Invisible
            .Join(magazineOrigin.DOLocalMove(new Vector3(0, -0.559f, 1.159f), 0.2f)) // Move to the right position
            .Join(recoilOrigin.DOPunchRotation(new Vector3(-2f, 0f, 0f), 0.2f, 10, 1.0f)) // Punch recoil to simulate magazine ejection
            .AppendCallback(() => magazineOrigin.localPosition = new Vector3(0, -0.554f, 1.25f))
            .Append(magazineOrigin.GetComponentInChildren<Renderer>().material.DOFade(1.0f, 0.3f))
            .Append(magazineOrigin.DOLocalMove(new Vector3(0, 0.384f, 1.101f), 0.2f))
            .JoinCallback(() => audioPlayer.PlaySound2())
            .Append(recoilOrigin.DOPunchRotation(new Vector3(-2f, 0f, 0f), 0.2f, 10, 1.0f)) // Punch recoil to simulate magazine ejection
            .JoinCallback(() => reloading.curMag = reloading.maxMagSize)
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0f, 0f, 65f), 0.25f))
            .Append(bolt.DOLocalMove(new Vector3(0.03f, 0.53f, 1.1f), 0.17f))
            .JoinCallback(() => audioPlayer.PlaySound3())
            .AppendInterval(0.2f)
            .Append(bolt.DOLocalMove(new Vector3(0.03f, 0.53f, 1.418f), 0.17f))
            .JoinCallback(() => audioPlayer.PlaySound4())
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.3f));
    }
    public override void Fire()
    {
        fireSequence.Restart();
    }

    public override bool IsReloading()
    {
        return reloadSequence.IsPlaying();
    }

    public override void Reload()
    {
        reloadSequence.Restart();
    }
}
