using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dashing : MonoBehaviour
{
    AnimationController animController;
    Movement movementScript;
    
    [SerializeField] float dashDelay = 1;
    [SerializeField] [Range(0, 15000)] float dashForce;

    [SerializeField] Transform cameraContainer;
    
    float lastDashTime;
    Rigidbody rig;

    void Awake()
    {
        animController = GetComponent<AnimationController>();
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
        // Get Movement Script Reference
        movementScript = GetComponent<Movement>();
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
        switch (movementScript.moveDirection)
        {
            case Movement.MoveDirection.Forward:
                rig.AddForce(cameraContainer.forward * dashForce, ForceMode.Impulse);
                animController.DashForward();
                break;
            case Movement.MoveDirection.ForwardRight:
                rig.AddForce((cameraContainer.forward + cameraContainer.right) * dashForce, ForceMode.Impulse);
                animController.DashForwardRight();
                break;
            case Movement.MoveDirection.Right:
                rig.AddForce(transform.right * dashForce, ForceMode.Impulse);
                animController.DashRight();
                break;
            case Movement.MoveDirection.BackRight:
                rig.AddForce((-cameraContainer.forward + cameraContainer.right) * dashForce, ForceMode.Impulse);
                animController.DashBackLeft();
                break;
            case Movement.MoveDirection.Back:
                rig.AddForce(-cameraContainer.forward * dashForce, ForceMode.Impulse);
                animController.DashBack();
                break;
            case Movement.MoveDirection.BackLeft:
                rig.AddForce((-cameraContainer.forward + -cameraContainer.right) * dashForce, ForceMode.Impulse);
                animController.DashBackLeft();
                break;
            case Movement.MoveDirection.Left:
                rig.AddForce(-cameraContainer.right * dashForce, ForceMode.Impulse);
                animController.DashLeft();
                break;
            case Movement.MoveDirection.ForwardLeft:
                rig.AddForce((cameraContainer.forward + -cameraContainer.right) * dashForce, ForceMode.Impulse);
                animController.DashForwardLeft();
                break;
        }

        lastDashTime = Time.time;
    }
}
