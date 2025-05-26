using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Dashing : MonoBehaviour
{
    [Header("Force-Related Settings")]
    [SerializeField] [Range(0, 15000)] float dashForce;
    [SerializeField][Range(0, 3)] float dashVelocityMultiplier;

    [Header("Time-Related Settings")]
    [SerializeField] float dashDelay = 1;
    [SerializeField] float dashTime;
    float lastDashTime;
    
    [Header("References")]
    [SerializeField] Transform dashCameraContainer;
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
        if (context.phase == InputActionPhase.Performed && (Time.time - lastDashTime > dashDelay))
        {
            StartCoroutine(StartDash());
        }   
    }

    IEnumerator StartDash()
    {
        MoveDirection moveDirection = commonVariables.GetMoveDirection();
        onDash.Invoke(moveDirection);

        Vector3 startVelocity = rig.velocity;
        switch (moveDirection)
        {
            case MoveDirection.Forward:
                rig.AddForce(dashCameraContainer.forward * dashForce, ForceMode.Impulse);
                break;
            case MoveDirection.ForwardRight:
                rig.AddForce((dashCameraContainer.forward + dashCameraContainer.right) * dashForce, ForceMode.Impulse);
                break;
            case MoveDirection.Right:
                rig.AddForce(transform.right * dashForce, ForceMode.Impulse);
                break;
            case MoveDirection.BackRight:
                rig.AddForce((-dashCameraContainer.forward + dashCameraContainer.right) * dashForce, ForceMode.Impulse);
                break;
            case MoveDirection.Back:
                rig.AddForce(-dashCameraContainer.forward * dashForce, ForceMode.Impulse);
                break;
            case MoveDirection.BackLeft:
                rig.AddForce((-dashCameraContainer.forward + -dashCameraContainer.right) * dashForce, ForceMode.Impulse);
                break;
            case MoveDirection.Left:
                rig.AddForce(-dashCameraContainer.right * dashForce, ForceMode.Impulse);
                break;
            case MoveDirection.ForwardLeft:
                rig.AddForce((dashCameraContainer.forward + -dashCameraContainer.right) * dashForce, ForceMode.Impulse);
                break;
        }
        yield return new WaitForSeconds(dashTime);
        rig.velocity = startVelocity * dashVelocityMultiplier;
        lastDashTime = Time.time;
    }
}
