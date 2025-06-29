using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Dashing : MonoBehaviour
{
    [Header("Force-Related Settings")]
    [SerializeField] [Range(0, 300)] float dashForce;
    [SerializeField][Range(0, 3)] float dashVelocityMultiplier;

    [Header("Time-Related Settings")]
    [SerializeField] float dashDelay = 1;
    [SerializeField] float dashTime;
    float lastDashTime;
    bool dashing;
    
    [Header("References")]
    [SerializeField] Transform aimingCameraContainer;
    Rigidbody rig;
    CommonVariables commonVariables;

    [Header("Events")]
    public UnityEvent<MoveDirection> onDash = new UnityEvent<MoveDirection>();

    void Awake()
    {
        // get component references
        rig = GetComponent<Rigidbody>();
        commonVariables = GetComponent<CommonVariables>();
    }
    
    public void OnDashInput(InputAction.CallbackContext context)
    {
        // Check if dash input pressed and dash delay passed
        if (context.phase == InputActionPhase.Performed && (Time.time > lastDashTime + dashDelay) && !dashing)
        {
            StartCoroutine(StartDash());
        }
    }

    IEnumerator StartDash()
    {
        dashing = true;
        Vector3 startVelocity = rig.velocity;

        MoveDirection moveDirection = commonVariables.GetMoveDirection();
        onDash.Invoke(moveDirection);

        switch (moveDirection)
        {
            case MoveDirection.Forward:
                rig.AddForce(aimingCameraContainer.forward * dashForce, ForceMode.Acceleration);
                break;

            case MoveDirection.ForwardRight:
                rig.AddForce((aimingCameraContainer.forward + aimingCameraContainer.right) * dashForce, ForceMode.Acceleration);
                break;

            case MoveDirection.Right:
                rig.AddForce(transform.right * dashForce, ForceMode.Acceleration);
                break;

            case MoveDirection.BackRight:
                rig.AddForce((-aimingCameraContainer.forward + aimingCameraContainer.right) * dashForce, ForceMode.Acceleration);
                break;

            case MoveDirection.Back:
                rig.AddForce(-aimingCameraContainer.forward * dashForce, ForceMode.Acceleration);
                break;

            case MoveDirection.BackLeft:
                rig.AddForce((-aimingCameraContainer.forward + -aimingCameraContainer.right) * dashForce, ForceMode.Acceleration);
                break;

            case MoveDirection.Left:
                rig.AddForce(-aimingCameraContainer.right * dashForce, ForceMode.Acceleration);
                break;

            case MoveDirection.ForwardLeft:
                rig.AddForce((aimingCameraContainer.forward + -aimingCameraContainer.right) * dashForce, ForceMode.Acceleration);
                break;
        }
        
        yield return new WaitForSeconds(dashTime);

        rig.velocity = startVelocity * dashVelocityMultiplier;

        dashing = false;
        lastDashTime = Time.time;
    }
}
