using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jumping : MonoBehaviour
{
    [SerializeField] PlayerMain playerMain;

    [SerializeField] float jumpForce;
    [SerializeField] int airJumps;

    int remainingAirJumps;

    Rigidbody rig;
    
    void Awake()
    {
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
        // Set Default AirJumps
        remainingAirJumps = airJumps;
    }
    void FixedUpdate()
    {
        // If we are grounded
        if (playerMain.groundState != PlayerMain.GroundState.Airborne)
        {
            // Reset AirJumps
            remainingAirJumps = airJumps;
        }
    }
    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // Determine Which Type of Jump to do
            switch (playerMain.groundState)
            {
                case PlayerMain.GroundState.Grounded:
                    GroundedJump();
                    break;
                case PlayerMain.GroundState.Airborne:
                    AirborneJump();
                    break;
                case PlayerMain.GroundState.WallGrounded:
                    WallJump();
                    break;
            }
        }
    }

    void GroundedJump()
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
        switch (playerMain.wallState)
        {
            case PlayerMain.WallState.Right:
                rig.AddForce((-transform.right + transform.up) * jumpForce, ForceMode.Impulse);
                break;
            case PlayerMain.WallState.Left:
                rig.AddForce((transform.right + transform.up) * jumpForce, ForceMode.Impulse);
                break;
            case PlayerMain.WallState.Front:
                rig.AddForce((-transform.forward + transform.up) * jumpForce, ForceMode.Impulse);
                break;
            case PlayerMain.WallState.Back:
                rig.AddForce((transform.forward + transform.up) * jumpForce, ForceMode.Impulse);
                break;
        }
    }
}
