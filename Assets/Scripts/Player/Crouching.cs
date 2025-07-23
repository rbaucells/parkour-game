using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;
using UnityEngine.Events;

public class Crouching : MonoBehaviour
{
    public enum SlamAction
    {
        None,
        Explode,
        Implode
    }

    [Header("Ground Slam Action")]
    [SerializeField] SlamAction slamAction = SlamAction.None;

    bool canSlam;

    [ShowIf(nameof(IsSlamActionExplodeOrImplode))] public float actionRadius;
    [ShowIf(nameof(IsSlamActionExplodeOrImplode))] public float actionForce;
    [ShowIf(nameof(IsSlamActionExplode))] public float explosionUpForce;

    [SerializeField] float groundSlamForce;

    [Header("Slide")]
    [SerializeField] float constantMoveSpeed;
    [SerializeField] float startImpulse;
    [SerializeField] float maxSlideSpeed;
    bool waitForGroundSlide;

    [Header("Misc")]
    [SerializeField] float crouchDownForce;
    [SerializeField] float unCrouchRaycastLengh;
    bool buttonHeld;

    [Header("References")]
    [SerializeField] LayerMask unCrouchLayerMask;
    [SerializeField] Transform crouchCameraContainer;

    [Header("Events")]
    public UnityEvent onCrouch = new UnityEvent();
    public UnityEvent onSlide = new UnityEvent();
    public UnityEvent onUnCrouchSlide = new UnityEvent();
    public UnityEvent onGroundSlam = new UnityEvent();
    Rigidbody rig;
    CommonVariables commonVariables;

    void Awake()
    {
        // get references
        rig = GetComponent<Rigidbody>();
        commonVariables = GetComponent<CommonVariables>();
    }

    public void OnCrouchInput(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                buttonHeld = true;  
                if (commonVariables.GetMoveDirection() == MoveDirection.None)
                {
                    switch (commonVariables.GetGroundState())
                    {
                        case GroundState.Grounded:
                            StartCrouchOnGround();
                            break;
                        case GroundState.Airborne:
                            StartCrouchInAir();
                            break;
                    }
                }
                else
                {
                    switch (commonVariables.GetGroundState())
                    {
                        case GroundState.Grounded:
                            StartSlideOnGround();
                            break;
                        case GroundState.Airborne:
                            StartSlideInAir();
                            break;
                    }
                }
                break;
            case InputActionPhase.Canceled:
                buttonHeld = false;
                StopCrouchSlide();
                break;
        } 
    }

    void FixedUpdate()
    {
        switch (commonVariables.GetCrouchState())
        {
            case CrouchState.Crouched:
                switch (commonVariables.GetGroundState())
                {
                    case GroundState.Grounded:
                        WhileCrouchOnGround();
                        break;
                    case GroundState.Airborne:
                        WhileCrouchInAir();
                        break;
                }

                break;
            case CrouchState.Sliding:
                switch (commonVariables.GetGroundState())
                {
                    case GroundState.Grounded:
                        WhileSlideOnGround();
                        break;
                }

                break;
        }

        if (waitForGroundSlide)
        {
            WhileSlideInAir();
        }
    }

    void StartCrouchOnGround()
    {
        commonVariables.SetCrouchState(CrouchState.Crouched);
        onCrouch.Invoke();
    }

    void StartCrouchInAir()
    {
        commonVariables.SetCrouchState(CrouchState.Crouched);
        onCrouch.Invoke();
        canSlam = true;
    }

    void StartSlideOnGround()
    {
        commonVariables.SetCrouchState(CrouchState.Sliding);
        onSlide.Invoke();
        Vector2 moveInput = commonVariables.GetMoveInput();
        Vector3 startDirection = new Vector3(moveInput.x, 0, moveInput.y);
        commonVariables.SetSlidingDirection(startDirection);
        if (rig.velocity.magnitude < maxSlideSpeed)
            rig.AddRelativeForce(startDirection * startImpulse, ForceMode.Impulse);
    }

    public void StartAirSlideOnGround()
    {
        if (waitForGroundSlide)
        {
            StartSlideOnGround();
        }
    }

    void StartSlideInAir()
    {
        waitForGroundSlide = true;
    }

    void WhileCrouchOnGround()
    {

    }

    void WhileSlideOnGround()
    {
        Vector3 startDirection = commonVariables.GetSlidingDirection();
        if (rig.velocity.magnitude < maxSlideSpeed)
        {
            rig.AddRelativeForce(startDirection * constantMoveSpeed, ForceMode.Acceleration);
        }
    }

    void WhileCrouchInAir()
    {
        if (canSlam)
            rig.AddForce(Vector3.down * crouchDownForce, ForceMode.Acceleration);
    }

    void WhileSlideInAir()
    {

    }

    public void Slam(Collision other)
    {
        Debug.Log("slam");
        if (!canSlam)
            return;
        canSlam = false;
        // bouncy
        rig.AddForce(other.GetContact(0).normal * groundSlamForce, ForceMode.Impulse);

        // actionate
        switch (slamAction)
        {
            case SlamAction.Explode:
                Boom.Explode(other.GetContact(0).point, actionRadius, actionForce, explosionUpForce);
                break;
            case SlamAction.Implode:
                Boom.Implode(other.GetContact(0).point, actionRadius, actionForce);
                break;
        }

        onGroundSlam.Invoke();
    }

    void StopCrouchSlide()
    {
        if (Physics.Raycast(crouchCameraContainer.position, transform.up, unCrouchRaycastLengh, unCrouchLayerMask))
        {
            StartCoroutine(WaitUntilAbleToUnCrouch());
            return;
        }

        commonVariables.SetCrouchState(CrouchState.Standing);
        onUnCrouchSlide.Invoke();
        canSlam = false;
        waitForGroundSlide = false;
    }

    IEnumerator WaitUntilAbleToUnCrouch()
    {
        yield return new WaitUntil(() => !Physics.Raycast(transform.position, transform.up, unCrouchRaycastLengh, unCrouchLayerMask));
        if (!buttonHeld)
            StopCrouchSlide();
    }

    // Helper methods for NaughtyAttributes
    bool IsSlamActionExplodeOrImplode()
    {
        return slamAction == SlamAction.Explode || slamAction == SlamAction.Implode;
    }

    bool IsSlamActionExplode()
    {
        return slamAction == SlamAction.Explode;
    }
}
