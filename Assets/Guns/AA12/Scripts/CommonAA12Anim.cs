using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CommonAA12Anim : AbstractGunAnimator
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
            .Append(magOrigin.DOLocalMove(new Vector3(0, -0.554f, 1.025f), 0.4f))
            .Join(magMat.DOFade(0, 0.25f))
            .JoinCallback(() => PlayAudio.Invoke(0))
            .Append(magOrigin.DOLocalMove(new Vector3(0, -0.647f, 1.197f), 0.15f))
            .Append(magOrigin.DOLocalMove(new Vector3(0, 0.398f, 0.978f), 0.25f))
            .Join(magMat.DOFade(1, 0.15f))
            .JoinCallback(() => onReload.Invoke())
            .JoinCallback(() => PlayAudio.Invoke(1))
            .Append(recoilOrigin.DOPunchRotation(new Vector3(-4f, 0f, 0f), 0.2f))
            // .Append(recoilOrigin.DOLocalRotate(new Vector3(0, 0, 62), 0.22f))
            .Append(chargingHandle.DOLocalMove(new Vector3(0.03f, 0.55f, 0.964f), 0.2f))
            .JoinCallback(() => PlayAudio.Invoke(2))
            .Append(recoilOrigin.DOPunchRotation(new Vector3(-1, 0, 0), 0.22f))
            .AppendInterval(0.15f)
            .Append(chargingHandle.DOLocalMove(new Vector3(0.03f, 0.55f, 1.255f), 0.15f))
            .JoinCallback(() => PlayAudio.Invoke(2))
            .Append(recoilOrigin.DOPunchRotation(new Vector3(1, 0, 0), 0.22f))
            // .Append(recoilOrigin.DOLocalRotate(new Vector3(0, 0, 0), 0.16f))
            .SetAutoKill(false);

        fireSequence
            .Append(recoilOrigin.DOLocalRotate(new Vector3(-7, 0, 0), 0.1f))
            .Join(chargingHandle.DOLocalMove(new Vector3(0.03f, 0.55f, 0.964f), 0.06f))
            .Join(recoilOrigin.DOLocalMove(new Vector3(0, -0.372f, -1.4f), 0.12f))
            .Append(chargingHandle.DOLocalMove(new Vector3(0.03f, 0.55f, 1.255f), 0.06f))
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
