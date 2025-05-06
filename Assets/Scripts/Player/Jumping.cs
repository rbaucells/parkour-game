using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jumping : MonoBehaviour
{
    [HideInInspector] public bool jumpHeld;
    [SerializeField] float groundJumpForce;
    [SerializeField] float airJumpForce;
    [SerializeField] float wallJumpForce;
    GroundCheck groundCheckScript;

    [Range(0,1)] [SerializeField] float cayoteTime;

    [HideInInspector] public bool usedCayoteTime;

    public int maxAirJumps;
    [HideInInspector] public int remainingAirJumps;

    Rigidbody rig;
    AnimationController animController;
    
    void Awake()
    {
        animController = GetComponent<AnimationController>();
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
        Debug.Log("Ground Jump");
        rig.AddForce(transform.up * groundJumpForce, ForceMode.Impulse);

        usedCayoteTime = true;

        animController.Jump();
    }

    void AirborneJump()
    {
        Debug.Log("Airborn Jump");
        if (groundCheckScript.lastGroundedTime + cayoteTime > Time.time && !usedCayoteTime)
        {
            if (rig.velocity.y < 0)
            {
                rig.velocity = new(rig.velocity.x, 0, rig.velocity.z);
            }
            rig.AddForce(transform.up * groundJumpForce, ForceMode.Impulse);
            usedCayoteTime = true;

            animController.Jump();
        }
        // If we Haven't ran out of AirJumps
        else if (remainingAirJumps > 0)
        {
            if (rig.velocity.y < 0)
            {
                rig.velocity = new(rig.velocity.x, 0, rig.velocity.z);
            }

            rig.AddForce(transform.up * airJumpForce, ForceMode.Impulse);
            remainingAirJumps--;

            animController.Jump();
        }
    }

    void WallJump()
    {
        usedCayoteTime = true;

        Debug.Log("Wall Jump");
        // Jump Depending on WallSide
        switch (groundCheckScript.wallState)
        {
            case GroundCheck.WallState.Right:
                rig.AddForce((-transform.right + transform.up) * wallJumpForce, ForceMode.Impulse);
                break;
            case GroundCheck.WallState.Left:
                rig.AddForce((transform.right + transform.up) * wallJumpForce, ForceMode.Impulse);
                break;
            case GroundCheck.WallState.Front:
                rig.AddForce((-transform.forward + transform.up) * wallJumpForce, ForceMode.Impulse);
                break;
            case GroundCheck.WallState.Back:
                rig.AddForce((transform.forward + transform.up) * wallJumpForce, ForceMode.Impulse);
                break;
        }

        animController.Jump();
    }
}
