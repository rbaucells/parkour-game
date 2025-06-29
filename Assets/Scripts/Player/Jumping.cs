using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Unity.VisualScripting;

public class Jumping : MonoBehaviour
{
    [Header("Jump Count")]
    [SerializeField] int maxAirJumps;
    int remainingAirJumps;
    bool jumpHeld;
    
    [Header("Force Settings")]
    [SerializeField] float groundJumpForce;
    [SerializeField] float airJumpForce;
    [SerializeField] float wallJumpForce;
    [Header("Cayote Time")]
    [Range(0, 1)] [SerializeField] float cayoteTime;
    bool usedCayoteTime;
    float lastGroundedTime;

    [Header("Jump Buffering")]
    [SerializeField] float raycastLenght;
    [SerializeField] LayerMask layerMask;
    bool inJumpBuffer = false;
    Coroutine jumpBufferCoroutine;

    [Header("Events")]
    public UnityEvent onGroundJump = new UnityEvent();
    public UnityEvent onWallJump = new UnityEvent();
    public UnityEvent onAirJump = new UnityEvent();

    // component references
    Rigidbody rig;
    CommonVariables commonVariables;
    CapsuleCollider capsuleCollider;
    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        commonVariables = GetComponent<CommonVariables>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        remainingAirJumps = maxAirJumps;
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            switch (commonVariables.GetGroundState())
            {
                case GroundState.Grounded:
                    GroundedJump();
                    break;
                case GroundState.Airborne:
                    AirborneJump();
                    break;
                case GroundState.WallGrounded:
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
        rig.AddForce(transform.up * groundJumpForce, ForceMode.Impulse);
        onGroundJump.Invoke();
        usedCayoteTime = true;
    }

    void AirborneJump()
    {

        if (lastGroundedTime + cayoteTime > Time.time && !usedCayoteTime)
        {
            if (rig.velocity.y < 0)
            {
                rig.velocity = new(rig.velocity.x, 0, rig.velocity.z);
            }
            rig.AddForce(transform.up * groundJumpForce, ForceMode.Impulse);
            usedCayoteTime = true;

            onAirJump.Invoke();
        }
        else if (Physics.Raycast(new Vector3(transform.position.x, (transform.position.y + capsuleCollider.center.y) - capsuleCollider.height / 2, transform.position.z), Vector3.down, raycastLenght, layerMask) && !inJumpBuffer)
        {
            jumpBufferCoroutine = StartCoroutine(JumpBuffer());
        }
        else if (remainingAirJumps > 0)
        {
            if (rig.velocity.y < 0)
            {
                rig.velocity = new(rig.velocity.x, 0, rig.velocity.z);
            }

            rig.AddForce(transform.up * airJumpForce, ForceMode.Impulse);
            remainingAirJumps--;

            onAirJump.Invoke();
        }
    }

    IEnumerator JumpBuffer()
    {
        inJumpBuffer = true;
        yield return new WaitUntil(() => commonVariables.GetGroundState() == GroundState.Grounded);
        GroundedJump();
        inJumpBuffer = false;
    }

    public void CancelJumpBuffer()
    {
        if (inJumpBuffer)
        {
            StopCoroutine(jumpBufferCoroutine);
        }
    }

    void WallJump()
    {
        onWallJump.Invoke();
        usedCayoteTime = true;

        switch (commonVariables.GetWallState())
        {
            case WallState.Right:
                rig.AddForce((-transform.right + transform.up) * wallJumpForce, ForceMode.Impulse);
                break;
            case WallState.Left:
                rig.AddForce((transform.right + transform.up) * wallJumpForce, ForceMode.Impulse);
                break;
            case WallState.Front:
                rig.AddForce((-transform.forward + transform.up) * wallJumpForce, ForceMode.Impulse);
                break;
            case WallState.Back:
                rig.AddForce((transform.forward + transform.up) * wallJumpForce, ForceMode.Impulse);
                break;
        }
    }

    public void JumpIfHeld()
    {
        if (jumpHeld)
            GroundedJump();
        RegenerateAirJumpsAndCayote();
    }

    public void RegenerateAirJumpsAndCayote()
    {
        remainingAirJumps = maxAirJumps;
        usedCayoteTime = false;
    }

    public void SetLastGroundTime()
    {
        lastGroundedTime = Time.time;
    }
}
