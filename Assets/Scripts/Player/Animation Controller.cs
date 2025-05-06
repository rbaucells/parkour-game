using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    [SerializeField] Transform crouchContainer;

    Sequence jumpSequence;
    Sequence wallRunRightIn;
    Sequence wallRunLeftIn;
    Sequence wallRunFrontIn;
    Sequence wallRunBackIn;
    Sequence wallRunOut;
    void Start()
    {
        jumpSequence = DOTween.Sequence()
            .Append(jumpContainer.DOLocalRotate(new Vector3(-4f, 0f, 0f), 0.18f))
            .Append(jumpContainer.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.17f))
            .SetAutoKill(false)
            .SetEase(Ease.OutSine);
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

    public void WallRunIn(GroundCheck.WallState wallState)
    {
        switch (wallState)
        {
            case GroundCheck.WallState.Right:
                WallRunRightIn();
                break;
            case GroundCheck.WallState.Left:
                WallRunLeftIn();
                break;
            case GroundCheck.WallState.Front:
                WallRunFrontIn();
                break;
            case GroundCheck.WallState.Back:
                WallRunBackIn();
                break;
        }
    }

    void WallRunRightIn()
    {   
        wallRunLeftIn.Kill();
        wallRunOut.Kill();
        wallRunBackIn.Kill();
        wallRunFrontIn.Kill();

        wallRunRightIn = DOTween.Sequence()
            .Append(wallConatiner.DOLocalRotate(new Vector3(0, -5f, 5f), 0.3f))
            .SetAutoKill(true)
            .Play();
    }

    void WallRunFrontIn()
    {
        wallRunLeftIn.Kill();
        wallRunRightIn.Kill();
        wallRunOut.Kill();
        wallRunBackIn.Kill();

        wallRunFrontIn = DOTween.Sequence()
            .Append(wallConatiner.DOLocalRotate(new Vector3(-10, 0, 0), 0.3f))
            .SetAutoKill(true)
            .Play();
    }

    void WallRunBackIn()
    {
        wallRunLeftIn.Kill();
        wallRunRightIn.Kill();
        wallRunOut.Kill();
        wallRunFrontIn.Kill();

        wallRunBackIn = DOTween.Sequence()
            .Append(wallConatiner.DOLocalRotate(new Vector3(10, 0, 0), 0.3f))
            .SetAutoKill(true)
            .Play(); 
    }
    
    void WallRunLeftIn()
    {
        wallRunRightIn.Kill();
        wallRunOut.Kill();
        wallRunBackIn.Kill();
        wallRunFrontIn.Kill();

        wallRunLeftIn = DOTween.Sequence()
            .Append(wallConatiner.DOLocalRotate(new Vector3(0, 5f, -5f), 0.3f))
            .SetAutoKill(true)
            .Play();
    }
    public void WallRunOut()
    {
        wallRunLeftIn.Kill();
        wallRunRightIn.Kill();
        wallRunBackIn.Kill();
        wallRunFrontIn.Kill();

        wallRunOut = DOTween.Sequence()
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
        float curRadius = capsuleCollider.radius;
        DOVirtual.Float(curHeight, 0.5f, 0.2f, x => capsuleCollider.height = x);
        DOVirtual.Float(curCenter, -0.75f, 0.2f, x => capsuleCollider.center = new Vector3(0f, x, 0f));
        DOVirtual.Float(curRadius, 0.25f, 0.2f, x => capsuleCollider.radius = x);
        crouchContainer.DOLocalMove(new Vector3(0, -0.25f, 0), 0.2f);
    }
    public void UnCrouch()
    {
        float curHeight = capsuleCollider.height;
        float curCenter = capsuleCollider.center.y;
        float curRadius = capsuleCollider.radius;
        DOVirtual.Float(curHeight, 2f, 0.2f, x => capsuleCollider.height = x);
        DOVirtual.Float(curCenter, 0f, 0.2f, x => capsuleCollider.center = new Vector3(0, x, 0));
        DOVirtual.Float(curRadius, 0.5f, 0.2f, x => capsuleCollider.radius = x);
        crouchContainer.DOLocalMove(new Vector3(0, 1, 0), 0.2f);
    }
}
