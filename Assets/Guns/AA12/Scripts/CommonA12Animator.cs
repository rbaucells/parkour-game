using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CommonA12Animator : AbstractGunAnimator
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
            .Append(recoilOrigin.DOLocalRotate(new Vector3(-15f, 0.0f, 0.0f), 0.07f))
            .Join(recoilOrigin.DOLocalMove(new Vector3(0.0f,-0.5f, -1.4f), 0.05f))
            .Join(bolt.DOLocalMove(new Vector3(0.03f, 0.4f, 0.805f), 0.03f))
            .Append(bolt.DOLocalMove(new Vector3(0.03f, 0.4f, 1.13f), 0.03f))
            .Join(recoilOrigin.DOLocalRotate(new Vector3(0.0f, 0.0f, 0.0f), 0.25f))
            .Join(recoilOrigin.DOLocalMove(new Vector3(0f, -0.5f, -1.3f), 0.2f));

        reloadSequence
            .AppendCallback(() =>
            {
                magazineOrigin.localPosition = new Vector3(0, 0.37f, 1.2539f); // Set Starting Pos
                magazineOrigin.localRotation = Quaternion.Euler(-3.703f, 0, 0); // Set Starting Rot
            })
            .AppendCallback(() => audioPlayer.PlaySound1())
            .AppendCallback(() => magazineOrigin.GetComponent<Rigidbody>().isKinematic = false)
            .Append(magazineOrigin.GetComponentInChildren<Renderer>().material.DOFade(0.0f, 0.6f)) // Fade into Invisible
            .AppendCallback(() => {
                magazineOrigin.GetComponent<Rigidbody>().velocity = Vector3.zero; // No More Velocity
                magazineOrigin.GetComponent<Rigidbody>().angularVelocity = Vector3.zero; // No More Angular Velocity
                magazineOrigin.GetComponent<Rigidbody>().isKinematic = true;
                magazineOrigin.localPosition = new Vector3(0,-0.85f,1.33f);
                magazineOrigin.localRotation = Quaternion.Euler(-3.703f, 0, 0);
            })
            .Append(magazineOrigin.GetComponentInChildren<Renderer>().material.DOFade(1.0f, 0.3f))
            .Append(magazineOrigin.DOLocalMove(new Vector3(0, 0.37f, 1.2539f), 0.45f))
            .JoinCallback(() => audioPlayer.PlaySound2())
            .AppendCallback(() => reloading.curMag = reloading.maxMagSize)
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0f, 0f, 45f), 0.4f))
            .AppendCallback(() => audioPlayer.PlaySound3())
            .Append(bolt.DOLocalMove(new Vector3(0.03f, 0.4f, 0.805f), 0.17f))
            .AppendInterval(0.07f)
            .AppendCallback(() => audioPlayer.PlaySound4())
            .Append(bolt.DOLocalMove(new Vector3(0.03f, 0.4f, 1.13f), 0.1f))
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.35f));
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
