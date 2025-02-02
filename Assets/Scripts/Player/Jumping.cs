using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jumping : MonoBehaviour
{
    
    [HideInInspector] public bool jumpHeld;
    [SerializeField] float jumpForce;
    GroundCheck groundCheckScript;

    public int maxAirJumps;
    [HideInInspector] public int remainingAirJumps;

    Rigidbody rig;
    
    void Awake()
    {
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
        // Set Default AirJumps
        remainingAirJumps = maxAirJumps;
        // Get GroundCheck Script Reference
        groundCheckScript = GetComponent<GroundCheck>();
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // Determine Which Type of Jump to do
            switch (groundCheckScript.groundState)
            {
                case GroundCheck.GroundState.Grounded:
                    GroundedJump();
                    break;
                case GroundCheck.GroundState.Airborne:
                    AirborneJump();
                    break;
                case GroundCheck.GroundState.WallGrounded:
                    WallJump();
                    break;
            }

            jumpHeld = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            jumpHeld = false;
        }
    }

    public void GroundedJump()
    {
        rig.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void AirborneJump()
    {
        // If we Haven't ran out of AirJumps
        if (remainingAirJumps > 0)
        {
            rig.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            remainingAirJumps--;
        }
    }

    void WallJump()
    {
        // Jump Depending on WallSide
        switch (groundCheckScript.wallState)
        {
            case GroundCheck.WallState.Right:
                rig.AddForce((-transform.right + transform.up) * jumpForce, ForceMode.Impulse);
                break;
            case GroundCheck.WallState.Left:
                rig.AddForce((transform.right + transform.up) * jumpForce, ForceMode.Impulse);
                break;
            case GroundCheck.WallState.Front:
                rig.AddForce((-transform.forward + transform.up) * jumpForce, ForceMode.Impulse);
                break;
            case GroundCheck.WallState.Back:
                rig.AddForce((transform.forward + transform.up) * jumpForce, ForceMode.Impulse);
                break;
        }
    }
}
