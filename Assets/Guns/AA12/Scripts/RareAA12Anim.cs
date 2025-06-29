using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RareAA12Anim : AbstractGunAnimator
{
    Sequence fireSequence;
    Sequence reloadSequence;

    [SerializeField] Transform magOrigin;
    [SerializeField] Transform mag;
    [SerializeField] Transform recoilOrigin;
    [SerializeField] Transform chargingHandle;

    Material magMat;

    void Start()
    {
        fireSequence = DOTween.Sequence();
        reloadSequence = DOTween.Sequence();

        magMat = mag.GetComponent<Renderer>().material;
        reloadSequence
            .Append(magOrigin.DOLocalMove(new Vector3(-0.259000003f, 0.169f, 0.989000022f), 0.2f))
            .Join(magOrigin.DOLocalRotate(new Vector3(357.191986f, -1.06849996e-07f, 298.269531f), 0.2f))
            .Join(recoilOrigin.DOPunchRotation(new Vector3(0, 0, -35), 0.6f, 0, 0))
            .Join(magMat.DOFade(0, 0.25f))
            .JoinCallback(() => PlayAudio.Invoke(0))
            .Append(magOrigin.DOLocalMove(new Vector3(0.453000009f, 0.140000001f, 0.99000001f), 0.15f))
            .Join(magOrigin.DOLocalRotate(new Vector3(357.192017f, -1.06849996e-07f, 61.7300072f), 0.15f))
            .Append(magOrigin.DOLocalMove(new Vector3(0, 0.398f, 0.978f), 0.25f))
            .Join(magOrigin.DOLocalRotate(new Vector3(357.192017f, 0, 0), 0.32f))
            .Join(magMat.DOFade(1, 0.15f))
            .JoinCallback(() => onReload.Invoke())
            .JoinCallback(() => PlayAudio.Invoke(1))
            .Join(recoilOrigin.DOPunchRotation(new Vector3(0, 0, 20), 0.7f, 0, 0))
            .Append(chargingHandle.DOLocalMove(new Vector3(0.0309876762f,0.615066409f,1.01600003f), 0.2f))
            .JoinCallback(() => PlayAudio.Invoke(2))
            .Append(recoilOrigin.DOPunchRotation(new Vector3(-1, 0, 0), 0.22f))
            .AppendInterval(0.15f)
            .Append(chargingHandle.DOLocalMove(new Vector3(0.0309876762f,0.615066409f,1.29799998f), 0.15f))
            .JoinCallback(() => PlayAudio.Invoke(2))
            .Append(recoilOrigin.DOPunchRotation(new Vector3(1, 0, 0), 0.22f))
            .SetAutoKill(false);

        fireSequence
            .Append(recoilOrigin.DOLocalRotate(new Vector3(-7, 0, 0), 0.1f))
            .Join(chargingHandle.DOLocalMove(new Vector3(0.0309876762f,0.615066409f,1.01600003f), 0.06f))
            .Join(recoilOrigin.DOLocalMove(new Vector3(0, -0.372f, -1.5f), 0.11f))
            .Append(chargingHandle.DOLocalMove(new Vector3(0.0309876762f,0.615066409f,1.29799998f), 0.06f))
            .Join(recoilOrigin.DOLocalRotate(new Vector3(0, 0, 0), 0.12f))
            .Join(recoilOrigin.DOLocalMove(new Vector3(0,-0.372f,-1.255f), 0.12f))
            .SetAutoKill(false);
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
