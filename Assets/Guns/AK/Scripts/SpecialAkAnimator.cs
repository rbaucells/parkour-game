using DG.Tweening;
using UnityEngine;

public class SpecialAkAnimator : AbstractGunAnimator
{
    [SerializeField] Transform bolt;
    [SerializeField] Transform bolt2;
    [SerializeField] Transform magazineOrigin;
    [SerializeField] GameObject magazine;

    [SerializeField] Transform recoilOrigin;

    Material magMat;
    Rigidbody magRig;

    Sequence reloadSequence;
    Sequence fireSequence;
    void Start()
    {
        magRig = magazineOrigin.GetComponent<Rigidbody>();

        fireSequence = DOTween.Sequence();
        reloadSequence = DOTween.Sequence();

        magMat = magazine.GetComponent<MeshRenderer>().material;

        reloadSequence
            .AppendCallback(() =>
            {
                magazineOrigin.localPosition = new Vector3(0, 0.33f, 1.17f); // Set Starting Pos
                magazineOrigin.localRotation = Quaternion.Euler(0, -90, 0); // Set Starting Rot
            })
            .AppendCallback(() => PlayAudio.Invoke(0))
            .AppendCallback(() => 
            {
                magRig.isKinematic = false; // Turn on Physics
            })
            .Append(magMat.DOFade(0.0f, 0.3f)) // Fade into Invisible
            .AppendInterval(0.25f) // Delay
            .AppendCallback(() => 
            {
                magRig.velocity = Vector3.zero; // No More Velocity
                magRig.angularVelocity = Vector3.zero; // No More Angular Velocity
                magRig.isKinematic = true; // Turn off Physics
            })
            .AppendCallback(() => 
            {
                magazineOrigin.localPosition = new Vector3(0, -0.552f, 1.379f); // Set Pos while still Invisible
            })
            .AppendInterval(0.1f) // Delay
            .Append(magMat.DOFade(1.0f, 0.3f)) // Fade into Visible
            .AppendCallback(() => PlayAudio.Invoke(1))
            .Append(magazineOrigin.DOLocalMove(new Vector3(0, 0.33f, 1.17f), 0.35f)) // Move back to Final Pos
            .AppendCallback(() => onReload.Invoke())
            .AppendInterval(0.15f) // Delay
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0.0f,0f,55f), 0.2f)) // Rotate to see bolt
            .AppendCallback(() => PlayAudio.Invoke(2))
            .Append(bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 0.94f), 0.2f)) // Move Bolt
            .AppendCallback(() => PlayAudio.Invoke(3))
            .Append(bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 1.21f), 0.2f)) // Move Bolt
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0.0f,0f,-55f), 0.5f)) // Rotate back to other bolt
            .AppendCallback(() => PlayAudio.Invoke(2))
            .Append(bolt2.DOLocalMove(new Vector3(-0.11f, 0.6f, 1.21f), 0.2f)) // Move Bolt
            .AppendCallback(() => PlayAudio.Invoke(3))
            .Append(bolt2.DOLocalMove(new Vector3(-0.11f, 0.6f, 0.94f), 0.2f)) // Move Bolt
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0.0f,0f,0f), 0.2f)); // Rotate back to normal

        fireSequence
            .Append(recoilOrigin.DOLocalRotate(new Vector3(-1.0f,0f,0f), 0.07f)) // Recoil rot
            .Insert(0.0f, recoilOrigin.DOLocalMove(new Vector3(0f,-0.129f,-1.17f), 0.07f)) // Recoil move
            .Insert(0.0f, bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 0.94f), 0.05f)) // Move Bolt
            .Insert(0.0f, bolt2.DOLocalMove(new Vector3(-0.11f, 0.6f, 1.21f), 0.05f)) // Move Bolt
            .Append(recoilOrigin.DOLocalMove(new Vector3(0f,-0.129f,-1.19f), 0.06f)) // Recoil move back
            .Insert(0.15f, recoilOrigin.DOLocalRotate(new Vector3(1f,0f,0f), 0.2f)) // Recoil rot back
            .Insert(0.15f, bolt.DOLocalMove(new Vector3(0.11f, 0.6f, 1.21f), 0.05f)) // Move Bolt
            .Insert(0.15f, bolt2.DOLocalMove(new Vector3(-0.11f, 0.6f, 0.94f), 0.05f)) // Move Bolt
            .Append(recoilOrigin.DOLocalRotate(new Vector3(0f,0f,0f), 0.2f)) // Recoil rot back
            .Append(recoilOrigin.DOLocalMove(new Vector3(0f,-0.129f,-1.18f), 0.06f)); // Recoil move back
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