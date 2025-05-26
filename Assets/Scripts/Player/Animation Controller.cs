using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class AnimationController : MonoBehaviour
{    
    [Header("Camera Containers")]
    [SerializeField] Transform jumpCameraContainer;
    [SerializeField] Transform landCameraContainer;
    [SerializeField] Transform slamCameraContainer;
    [SerializeField] Transform wallCameraContainer;
    [SerializeField] Transform crouchCameraContainer;
    [SerializeField] Transform walkCameraContainer;

    [Header("Weapon Containers")]
    [SerializeField] Transform jumpWeaponContainer;
    [SerializeField] Transform landWeaponContainer;
    [SerializeField] Transform slamWeaponContainer;
    [SerializeField] Transform wallWeaponContainer;
    [SerializeField] Transform crouchWeaponContainer;
    [SerializeField] Transform walkWeaponContainer;

    // sequences
    Sequence jumpSequence;
    Sequence wallRunRightIn;
    Sequence wallRunLeftIn;
    Sequence wallRunFrontIn;
    Sequence wallRunBackIn;
    Sequence wallRunOut;    

    // component references
    CapsuleCollider capsuleCollider;
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();

        jumpSequence = DOTween.Sequence()
            .Append(jumpCameraContainer.DOLocalRotate(new Vector3(-2, 0, 0), 0.3f))
            .Join(jumpWeaponContainer.DOLocalRotate(new Vector3(0.1f, 0, 0), 0.25f))
            .Join(jumpWeaponContainer.DOLocalMove(new Vector3(0, 0, 0.02f), 0.28f))
            .AppendInterval(0.07f)
            .Append(jumpCameraContainer.DOLocalRotate(new Vector3(0, 0, 0), 0.25f))
            .Join(jumpWeaponContainer.DOLocalRotate(new Vector3(0, 0, 0), 0.28f))
            .Join(jumpWeaponContainer.DOLocalMove(new Vector3(0, 0, 0), 0.28f))
            .SetAutoKill(false)
            .SetEase(Ease.OutSine);
    }

    public void Jump()
    {
        jumpSequence.Restart();
    }
    Coroutine WalkingAnimCore;

    public void StartWalk(MoveDirection moveDirection, float speed)
    {
        float animDuration = 10f/speed;
        WalkingAnimCore = StartCoroutine(WalkingAnim(animDuration));
    }

    IEnumerator WalkingAnim(float animDuration)
    {
        while (true)
        {
            walkWeaponContainer.DOLocalMoveX(0.03f, 0.25f).SetEase(Ease.InOutSine);
            walkWeaponContainer.DOLocalMoveY(0.03f, 0.25f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(0.25f);
            walkWeaponContainer.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(0.25f);
            walkWeaponContainer.DOLocalMoveX(-0.03f, 0.25f).SetEase(Ease.InOutSine);
            walkWeaponContainer.DOLocalMoveY(0.03f, 0.25f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(0.25f);
            walkWeaponContainer.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void StopWalk()
    {
        StopCoroutine(WalkingAnimCore);

        walkWeaponContainer.DOKill();

        walkWeaponContainer.DOLocalMove(Vector3.zero, 0.2f)
            .SetEase(Ease.InOutSine)
            .Play();
    }
    public void Land()
    {  
        Sequence landSequence = DOTween.Sequence()
            .Append(landCameraContainer.DOLocalRotate(new Vector3(1.5f, 0, 0), 0.17f))
            .Join(landWeaponContainer.DOLocalRotate(new Vector3(0.1f , 0, 0), 0.22f))
            .Append(landCameraContainer.DOLocalRotate(new Vector3(0, 0, 0), 0.25f))
            .Join(landWeaponContainer.DOLocalRotate(new Vector3(-0.2f, 0, 0), 0.28f))
            .Append(landWeaponContainer.DOLocalRotate(new Vector3(0, 0, 0), 0.15f))
            .SetAutoKill(true)
            .SetEase(Ease.OutSine)
            .Play();
    }

    public void WallRunIn(WallState wallState)
    {
        switch (wallState)
        {
            case WallState.Right:
                WallRunRightIn();
                break;
            case WallState.Left:
                WallRunLeftIn();
                break;
            case WallState.Front:
                WallRunFrontIn();
                break;
            case WallState.Back:
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
            .Append(wallCameraContainer.DOLocalRotate(new Vector3(0, 0, 7), 0.3f))
            .Join(wallWeaponContainer.DOLocalRotate(new Vector3(0, 0, 7), 0.32f))
            .SetEase(Ease.InOutSine, 0, 0.5f)
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
            .Append(wallCameraContainer.DOLocalRotate(new Vector3(10, 0, 0), 0.3f))
            .Join(wallWeaponContainer.DOLocalRotate(new Vector3(10, 0, 0), 0.32f))
            .SetEase(Ease.InOutSine, 0, 0.5f)
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
            .Append(wallCameraContainer.DOLocalRotate(new Vector3(10, 0, 0), 0.3f))
            .Join(wallWeaponContainer.DOLocalRotate(new Vector3(10, 0, 0), 0.32f))
            .SetEase(Ease.InOutSine, 0, 0.5f)
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
            .Append(wallCameraContainer.DOLocalRotate(new Vector3(0, 0, -7), 0.3f))
            .Join(wallWeaponContainer.DOLocalRotate(new Vector3(0, 0, -7), 0.36f))
            .SetEase(Ease.InOutSine, 0, 0.5f)
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
            .Append(wallCameraContainer.DOLocalRotate(new Vector3(0, 0, 0), 0.3f))
            .Join(wallWeaponContainer.DOLocalRotate(new Vector3(0, 0, 0), 0.36f))
            .SetEase(Ease.OutSine, 0, 0.5f)
            .SetAutoKill(true)
            .Play();
    }


    public void GroundSlam()
    {
        Sequence slamSequence = DOTween.Sequence()
            .SetAutoKill(true)
            .SetEase(Ease.OutCubic)
            .Play();
    }

    public void Dash(MoveDirection moveDirection)
    {
        switch (moveDirection)
        {
            case MoveDirection.Forward:
                DashForward();
                break;
            case MoveDirection.ForwardRight:
                DashForwardRight();
                break;
            case MoveDirection.Right:
                DashRight();
                break;
            case MoveDirection.BackRight:
                DashBackRight();
                break;
            case MoveDirection.Back:
                DashBack();
                break;
            case MoveDirection.BackLeft:
                DashBackLeft();
                break;
            case MoveDirection.Left:
                DashLeft();
                break;
            case MoveDirection.ForwardLeft:
                DashForwardLeft();
                break;
        }
    }
    void DashForward()
    {

    }
    void DashForwardRight()
    {

    }
    void DashRight()
    {
        
    }
    void DashBackRight()
    {

    }
    void DashBack()
    {

    }
    void DashBackLeft()
    {

    }
    void DashLeft()
    {

    }
    void DashForwardLeft()
    {

    }
    public void Crouch()
    {
        float curHeight = capsuleCollider.height;
        float curCenter = capsuleCollider.center.y;
        float curRadius = capsuleCollider.radius;
        DOVirtual.Float(curHeight, 0.5f, 0.2f, x => capsuleCollider.height = x).SetEase(Ease.InOutCubic);
        DOVirtual.Float(curCenter, -0.75f, 0.2f, x => capsuleCollider.center = new Vector3(0f, x, 0f)).SetEase(Ease.InOutCubic);
        DOVirtual.Float(curRadius, 0.25f, 0.2f, x => capsuleCollider.radius = x).SetEase(Ease.InOutCubic);
        crouchCameraContainer.DOLocalMove(new Vector3(0, -0.25f, 0), 0.2f).SetEase(Ease.InOutSine, 0, 0.1f);
        crouchWeaponContainer.DOLocalMove(new Vector3(0, -0.25f, 0), 0.205f).SetEase(Ease.InOutSine, 0, 0.1f);
    }
    public void UnCrouch()
    {
        float curHeight = capsuleCollider.height;
        float curCenter = capsuleCollider.center.y;
        float curRadius = capsuleCollider.radius;
        DOVirtual.Float(curHeight, 2f, 0.2f, x => capsuleCollider.height = x).SetEase(Ease.InOutCubic);
        DOVirtual.Float(curCenter, 0f, 0.2f, x => capsuleCollider.center = new Vector3(0, x, 0)).SetEase(Ease.InOutCubic);
        DOVirtual.Float(curRadius, 0.5f, 0.2f, x => capsuleCollider.radius = x).SetEase(Ease.InOutCubic);
        crouchCameraContainer.DOLocalMove(new Vector3(0, 1, 0), 0.2f).SetEase(Ease.InOutSine, 0, 0.1f);
        crouchWeaponContainer.DOLocalMove(new Vector3(0, 1, 0), 0.205f).SetEase(Ease.InOutSine, 0, 0.1f);
    }
}
