using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dashing : MonoBehaviour
{
    [SerializeField] PlayerMain playerMain;

    float lastDashTime;

    [SerializeField] float dashDelay = 1;
    [SerializeField] [Range(0, 200)] float dashForce;

    [SerializeField] Transform cameraContainer;
    
    Rigidbody rig;

    void Awake()
    {
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
    }
    public void OnDashInput(InputAction.CallbackContext context)
    {
        // Check if dash input pressed and dash delay passed
        if (context.phase == InputActionPhase.Performed && (Time.time - lastDashTime > dashDelay))
        {
            StartDash();
        }   
    }

    void StartDash()
    {
        // Apply dash force in direction
        switch (playerMain.moveDirection)
        {
            case PlayerMain.MoveDirection.Forward:
                rig.AddForce(cameraContainer.forward * dashForce, ForceMode.Impulse);
                break;
            case PlayerMain.MoveDirection.ForwardRight:
                rig.AddForce((cameraContainer.forward + cameraContainer.right) * dashForce, ForceMode.Impulse);
                break;
            case PlayerMain.MoveDirection.Right:
                rig.AddForce(transform.right * dashForce, ForceMode.Impulse);
                break;
            case PlayerMain.MoveDirection.BackRight:
                rig.AddForce((-cameraContainer.forward + cameraContainer.right) * dashForce, ForceMode.Impulse);
                break;
            case PlayerMain.MoveDirection.Back:
                rig.AddForce(-cameraContainer.forward * dashForce, ForceMode.Impulse);
                break;
            case PlayerMain.MoveDirection.BackLeft:
                rig.AddForce((-cameraContainer.forward + -cameraContainer.right) * dashForce, ForceMode.Impulse);
                break;
            case PlayerMain.MoveDirection.Left:
                rig.AddForce(-cameraContainer.right * dashForce, ForceMode.Impulse);
                break;
            case PlayerMain.MoveDirection.ForwardLeft:
                rig.AddForce((cameraContainer.forward + -cameraContainer.right) * dashForce, ForceMode.Impulse);
                break;
        }

        lastDashTime = Time.time;
    }
}
