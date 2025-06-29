using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EpicAA12Anim : AbstractGunAnimator
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
            .Append(magOrigin.DOLocalMove(new Vector3(0f, 0.330000013f, 1.70700002f), 0.36f))
            .Join(magOrigin.DOLocalRotate(new Vector3(327.699646f, 0f, 0f), 0.4f))
            .Join(magMat.DOFade(0, 0.35f))
            .JoinCallback(() => PlayAudio.Invoke(0))
            .Append(magOrigin.DOLocalMove(new Vector3(0, 0.133000001f, 1.92400002f), 0.15f))
            .Join(magOrigin.DOLocalRotate(new Vector3(325.267761f, 0, 0), 0.1f))
            .Append(magOrigin.DOLocalMove(new Vector3(0, 0.432999998f, 1.56599998f), 0.25f))
            .Join(magMat.DOFade(1, 0.15f))
            .JoinCallback(() => onReload.Invoke())
            .Append(magOrigin.DOLocalRotate(new Vector3(357.192017f, 0, 0), 0.23f))
            .JoinCallback(() => PlayAudio.Invoke(1))
            .Append(recoilOrigin.DOPunchRotation(new Vector3(3f, 0f, 0f), 0.2f))
            .Append(chargingHandle.DOLocalMove(new Vector3(0.0309876762f,0.576066256f,1.27100003f), 0.2f))
            .JoinCallback(() => PlayAudio.Invoke(2))
            .Append(recoilOrigin.DOPunchRotation(new Vector3(-2, 0, 0), 0.22f))
            .AppendInterval(0.15f)
            .Append(chargingHandle.DOLocalMove(new Vector3(0.0309876762f,0.576066256f,1.52999997f), 0.15f))
            .JoinCallback(() => PlayAudio.Invoke(2))
            .Append(recoilOrigin.DOPunchRotation(new Vector3(2, 0, 0), 0.22f))
            .SetAutoKill(false);

        fireSequence
            .Append(recoilOrigin.DOLocalRotate(new Vector3(-7, 0, 0), 0.1f))
            .Join(chargingHandle.DOLocalMove(new Vector3(0.0309876762f,0.576066256f,1.27100003f), 0.06f))
            .Join(recoilOrigin.DOLocalMove(new Vector3(0, -0.372f, -1.4f), 0.12f))
            .Append(chargingHandle.DOLocalMove(new Vector3(0.0309876762f,0.576066256f,1.52999997f), 0.06f))
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
