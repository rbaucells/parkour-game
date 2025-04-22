using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Transform cameraContainer;
    [SerializeField] Transform weaponHolder;
    [SerializeField] Transform cameraHolder;
    [SerializeField] CapsuleCollider capsuleCollider;

    [SerializeField] Transform jumpContainer;
    [SerializeField] Transform landContainer;
    [SerializeField] Transform slamContainer;
    [SerializeField] Transform wallConatiner;

    Sequence jumpSequence;
    Sequence wallRunRightInSequence;
    Sequence wallRunLeftInSequence;
    
    void Start()
    {
        jumpSequence = DOTween.Sequence()
            .Append(jumpContainer.DOLocalRotate(new Vector3(-3f, 0f, 0f), 0.2f))
            .Append(jumpContainer.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.17f))
            .SetAutoKill(false)
            .SetEase(Ease.OutSine);

        wallRunLeftInSequence = DOTween.Sequence()
            .Append(wallConatiner.DOLocalRotate(new Vector3(0, 5f, -5f), 0.3f))
            .SetAutoKill(false);
        
        wallRunRightInSequence = DOTween.Sequence()
            .Append(wallConatiner.DOLocalRotate(new Vector3(0, -5f, 5f), 0.3f))
            .SetAutoKill(false);
    }

    public void Jump()
    {
        jumpSequence.Restart();
    }

    public void StartWalk()
    {

    }
    public void StopWalk()
    {

    }
    public void Land(float speed)
    {  
        Sequence landSequence = DOTween.Sequence()
            .Append(landContainer.DOLocalRotate(new Vector3(2 * (speed/30f), 0f, 0f), 0.15f))
            .Append(landContainer.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.1f))
            .SetAutoKill(true)
            .SetEase(Ease.OutSine)
            .Play();
    }

    public void WallRunRightIn()
    {
        wallConatiner.DOKill();
        Debug.Log("Wall Run Right Animation");
        wallRunRightInSequence.Restart();
    }

    public void WallRunFrontIn()
    {

    }

     public void WallRunBackIn()
    {
        
    }
    
    public void WallRunLeftIn()
    {
        wallConatiner.DOKill();
        Debug.Log("Wall Run Left Animation");
        wallRunLeftInSequence.Restart();
    }

    public void WallRunOut()
    {
        wallConatiner.DOKill();

        var wallRunOutSequence = DOTween.Sequence()
            .Append(wallConatiner.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.2f))
            .SetAutoKill(true)
            .Play();
    }

    public void GroundSlamMiddle(float speed)
    {
        Sequence middleSlamSequence = DOTween.Sequence()
            .Append(slamContainer.DOLocalRotate(new Vector3(4 * (speed/60), 0f, 0f), 0.35f))
            .Append(slamContainer.DOLocalRotate(new Vector3(-1f, 0f, 0f), 0.3f))
            .Append(slamContainer.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.2f))
            .SetAutoKill(true)
            .SetEase(Ease.OutCubic)
            .Play();
    }
    public void GroundSlamLeft(float speed)
    {
        Sequence leftSlamSequence = DOTween.Sequence()
            .Append(slamContainer.DOLocalRotate(new Vector3(4 * (speed/60), 0f, 4f), 0.35f))
            .Append(slamContainer.DOLocalRotate(new Vector3(-1f, 0f, 1f), 0.3f))
            .Append(slamContainer.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.2f))
            .SetAutoKill(true)
            .SetEase(Ease.OutCubic)
            .Play();
    }
    public void GroundSlamRight(float speed)
    {
        Sequence rightSlamSequence = DOTween.Sequence()
            .Append(slamContainer.DOLocalRotate(new Vector3(4 * (speed/60), 0f, -4f), 0.35f))
            .Append(slamContainer.DOLocalRotate(new Vector3(-1f, 0f, -1f), 0.3f))
            .Append(slamContainer.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.2f))
            .SetAutoKill(true)
            .SetEase(Ease.OutCubic)
            .Play();
    }
    public void DashForward()
    {

    }
    public void DashForwardRight()
    {

    }
    public void DashRight()
    {
        
    }
    public void DashBackRight()
    {

    }
    public void DashBack()
    {

    }
    public void DashBackLeft()
    {

    }
    public void DashLeft()
    {

    }
    public void DashForwardLeft()
    {

    }
    public void Crouch()
    {
        float curHeight = capsuleCollider.height;
        float curCenter = capsuleCollider.center.y;
        DOTween.To(() => curHeight, x => capsuleCollider.height = x, 1f, 0.2f);
        DOTween.To(() => curCenter, x => capsuleCollider.center = new Vector3(0f, x, 0), -0.45f, 0.2f);
        cameraContainer.DOLocalMove(new Vector3(0, 0f, 0), 0.2f);
    }
    public void UnCrouch()
    {
        float curHeight = capsuleCollider.height;
        float curCenter = capsuleCollider.center.y;
        DOVirtual.Float(curHeight, 2f, 0.2f, x => capsuleCollider.height = x);
        DOVirtual.Float(curCenter, 0f, 0.2f, x => capsuleCollider.center = new Vector3(0, x, 0));
        cameraContainer.DOLocalMove(new Vector3(0, 1f, 0), 0.2f);
    }
}
