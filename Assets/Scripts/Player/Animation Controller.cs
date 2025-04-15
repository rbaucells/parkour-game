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

    [SerializeField] Transform jumpLandSlamContainer;
    [SerializeField] Transform walkContainer;
    [SerializeField] Transform wallRunContainer;
    [SerializeField] Transform dashContainer;

    Sequence jumpSequence;
    Sequence wallRunRightInSequence;
    Sequence wallRunLeftInSequence;
    Sequence wallRunOutSequence;
    
    void Start()
    {
        jumpSequence = DOTween.Sequence()
            .Append(cameraHolder.DOLocalRotate(new Vector3(-4f,0f,0f), 0.4f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(-3f,0f,0f), 0.4f))
            .Append(cameraHolder.DOLocalRotate(new Vector3(0f,0f,0f), 0.6f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(0f,0f,0f), 0.6f))
            .SetAutoKill(false);
        
        wallRunLeftInSequence = DOTween.Sequence()
            .Append(cameraHolder.DOLocalRotate(new Vector3(0, 5f, -5f), 0.3f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(0, 5f, -5f), 0.3f))
            .SetAutoKill(false);
        
        wallRunRightInSequence = DOTween.Sequence()
            .Append(cameraHolder.DOLocalRotate(new Vector3(0, -5f, 5f), 0.3f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(0, -5f, 5f), 0.3f))
            .SetAutoKill(false);
    }

    public void Jump()
    {
        Debug.Log("Jump Animation");
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
        Debug.Log("Land Animation");
        var landSequence = DOTween.Sequence();

        landSequence
            .Append(cameraHolder.DOLocalRotate(new Vector3(4f * (Mathf.Clamp(speed, 0f, 50f)/30f) ,0,0), 0.4f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(3.5f * (Mathf.Clamp(speed, 0f, 50f)/30f),0,0), 0.4f))
            .Append(cameraHolder.DOLocalRotate(new Vector3(0f,0f,0f), 0.4f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(0f,0f,0f), 0.4f))
            .SetAutoKill(true)
            .Play();
    }

    public void WallRunRightIn()
    {
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
        Debug.Log("Wall Run Left Animation");
        wallRunLeftInSequence.Restart();
    }

    public void WallRunOut()
    {
        Debug.Log("Wall Run Out Animation");
        wallRunOutSequence = DOTween.Sequence()
            .Append(cameraHolder.DOLocalRotate(new Vector3(0, 0f, 0f), 0.2f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(0, 0f, 0f), 0.2f))
            .SetAutoKill(true)
            .Play();
    }

    public void GroundSlamMiddle(float speed)
    {
        var GroundSlamMiddleSequence = DOTween.Sequence();

        GroundSlamMiddleSequence
            .Append(cameraHolder.DOLocalRotate(new Vector3(10f * (Mathf.Clamp(speed, 0f, 80f) / 70f), 0, 0), 0.2f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(10f * (Mathf.Clamp(speed, 0f, 80f) / 70f), 0, 0), 0.2f))
            .Append(cameraHolder.DOLocalRotate(new Vector3(-2f, 0f, 0f), 0.4f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(-2f, 0f, 0f), 0.4f))
            .Append(cameraHolder.DOLocalRotate(Vector3.zero, 0.1f))
            .Join(weaponHolder.DOLocalRotate(Vector3.zero, 0.1f))
            .SetAutoKill(true)
            .Play();
    }
    public void GroundSlamLeft(float speed, float hoziontalSpeed)
    {
        var GroundSlamLeftSequence = DOTween.Sequence();

        GroundSlamLeftSequence
            .Append(cameraHolder.DOLocalRotate(new Vector3(10f * (Mathf.Clamp(speed, 0f, 80f) / 70f), 0, 3f * (Mathf.Clamp(hoziontalSpeed, 0f, 30f) / 20)), 0.4f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(10f * (Mathf.Clamp(speed, 0f, 80f) / 70f), 0, 3f * (Mathf.Clamp(hoziontalSpeed, 0f, 30f) / 20)), 0.4f))
            .Append(cameraHolder.DOLocalRotate(new Vector3(-2f, 0f, 0f), 0.4f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(-2f, 0f, 0f), 0.4f))
            .Append(cameraHolder.DOLocalRotate(Vector3.zero, 0.2f))
            .Join(weaponHolder.DOLocalRotate(Vector3.zero, 0.2f))
            .SetAutoKill(true)
            .Play();
    }
    public void GroundSlamRight(float speed, float hoziontalSpeed)
    {
        var groundSlamRightSequence = DOTween.Sequence();

        groundSlamRightSequence
            .Append(cameraHolder.DOLocalRotate(new Vector3(10f * (Mathf.Clamp(speed, 0f, 80f) / 70f), 0, -3f * (Mathf.Clamp(hoziontalSpeed, 0f, 30f) / 20)), 0.4f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(10f * (Mathf.Clamp(speed, 0f, 80f) / 70f), 0, -3f * (Mathf.Clamp(hoziontalSpeed, 0f, 30f) / 20)), 0.4f))
            .Append(cameraHolder.DOLocalRotate(new Vector3(-2f, 0f, 0f), 0.4f))
            .Join(weaponHolder.DOLocalRotate(new Vector3(-2f, 0f, 0f), 0.4f))
            .Append(cameraHolder.DOLocalRotate(Vector3.zero, 0.2f))
            .Join(weaponHolder.DOLocalRotate(Vector3.zero, 0.2f))
            .SetAutoKill(true)
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
