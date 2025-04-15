using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LegendaryAAAAA12Animator : AbstractGunAnimator
{
    Audio audioPlayer;
    Reloading reloading;
    Sequence reloadSequence;
    Sequence fireSequence;

    [SerializeField] Transform recoilOrigin;
    [SerializeField] Transform magazineOrigin;
    [SerializeField] Transform magazineOrigin2;
    [SerializeField] Transform bolt;
    void Start()
    {
        audioPlayer = GetComponent<Audio>();
        reloading = GetComponent<Reloading>();

        fireSequence = DOTween.Sequence();
        reloadSequence = DOTween.Sequence();

        fireSequence
            .Append(recoilOrigin.DOLocalRotate(new Vector3(-7f, 0f, 0.0f), 0.07f))
            .Join(recoilOrigin.DOLocalMove(new Vector3(0.0f,-0.5f, -1.425f), 0.05f))
            .Join(bolt.DOLocalMove(new Vector3(0.03f, 0.528f, 1.084f), 0.1f))
            .Append(bolt.DOLocalMove(new Vector3(0.03f, 0.563f, 3.922f), 0.1f))
            .Join(recoilOrigin.DOLocalRotate(new Vector3(0.0f, 0.0f, 0.0f), 0.25f))
            .Join(recoilOrigin.DOLocalMove(new Vector3(0f, -0.358f, -1.368f), 0.2f));

        reloadSequence
            .AppendCallback(() =>
            {
                magazineOrigin.localPosition = new Vector3(0, 0.384f, 1.101f); // Set Starting Pos
                magazineOrigin.localRotation = Quaternion.Euler(-3.55f, 0, 0); // Set Starting Rot
                magazineOrigin2.localPosition = new Vector3(0f, 0.344f, 2.881f); // Set Starting Pos
                magazineOrigin2.localRotation = Quaternion.Euler(-3.55f, 0, 0); // Set Starting Rot
            })
            .AppendCallback(() => audioPlayer.PlaySound1())
            .Append(magazineOrigin.GetComponentInChildren<Renderer>().material.DOFade(0.0f, 0.3f)) // Fade into Invisible
            .Join(magazineOrigin.DOLocalMove(new Vector3(0, -0.713f, 1.169f), 0.34f)) // Move to the right position
            .Join(magazineOrigin2.DOLocalMove(new Vector3(0, -0.78f, 2.95f), 0.34f)) // Move to the right position
            .Join(recoilOrigin.DOPunchRotation(new Vector3(-8f, 0f, 0f), 0.25f, 10, 1.0f)) // Punch recoil to simulate magazine ejection
            .Append(magazineOrigin2.DOLocalMove(new Vector3(0f, -2.704f, 1.313f), 0.4f))
            .Append(magazineOrigin.DOLocalMove(new Vector3(0, -0.78f, 2.95f), 0.34f))
            .Append(magazineOrigin.DOLocalMove(new Vector3(0f, 0.344f, 2.881f), 0.3f))
            .Join(magazineOrigin2.DOLocalMove(new Vector3(0f, 0.384f, 1.101f), 0.3f))
            .JoinCallback(() => audioPlayer.PlaySound2())
            .Append(recoilOrigin.DOPunchRotation(new Vector3(-2f, 0f, 0f), 0.2f, 10, 1.0f)) // Punch recoil to simulate magazine ejection
            .JoinCallback(() => reloading.curMag = reloading.maxMagSize)
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0f, 0f, 65f), 0.25f))
            .Append(bolt.DOLocalMove(new Vector3(0.03f, 0.528f, 1.084f), 0.3f))
            .JoinCallback(() => audioPlayer.PlaySound3())
            .AppendInterval(0.2f)
            .Append(bolt.DOLocalMove(new Vector3(0.03f, 0.563f, 3.922f), 0.17f))
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
