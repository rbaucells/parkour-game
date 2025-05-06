using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    AnimationController animController;
    public enum MoveState
    {
        Idle,
        Moving
    }

    public enum MoveDirection
    {
        Forward,
        ForwardRight,
        Right,
        BackRight,
        Back,
        BackLeft,
        Left,
        ForwardLeft,
        None
    }

    [SerializeField] float onGroundMoveSpeed = 5f;
    [SerializeField] float onGroundDrag;
    [SerializeField] PhysicMaterial onGroundPhysicsMaterial;
    
    [SerializeField] float inAirMoveSpeed;
    [SerializeField] float inAirDrag;

    [SerializeField] float onWallMoveSpeed;
    [SerializeField] float onWallDrag;
    [SerializeField] PhysicMaterial onWallPhysicsMaterial;

    [SerializeField] float crouchMoveSpeed;
    [SerializeField] float crouchDrag;
    [SerializeField] PhysicMaterial crouchPhysicsMaterial;

    MoveState moveState = MoveState.Idle;
    [HideInInspector] public MoveDirection moveDirection { get; private set; } = MoveDirection.None;

    const float MOVE_THRESHOLD = 0.1f;

    [SerializeField] Transform cameraContainer;
    
    [HideInInspector] public bool crouching;

    GroundCheck groundCheckScript;
    Vector2 moveInputValue;
    Rigidbody rig;
    CapsuleCollider capsuleCollider;

    void Awake()
    {
        animController = GetComponent<AnimationController>();
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
        // Get GroundCheck Script Reference
        groundCheckScript = GetComponent<GroundCheck>();

        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        // Store the input value
        if (context.phase == InputActionPhase.Performed)
        {
            moveInputValue = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            moveInputValue = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        bool curMoving = moveInputValue.magnitude > MOVE_THRESHOLD;
        // If we are moving
        if (curMoving)
        {
            // If we were idle, start moving
            if (moveState == MoveState.Idle)
            {
                StartMoving();
            }
            // Set move state
            moveState = MoveState.Moving;

            WhileMoving();
        }
        else
        {
            // If we were moving, stop moving
            if (moveState == MoveState.Moving)
            {
                StopMoving();
            }

            moveState = MoveState.Idle;
        }
    }

    void StartMoving()
    {
        // Start looping move anim
        animController.StartWalk();
    }

    void WhileMoving()
    {
        switch (groundCheckScript.groundState)
        {
            case GroundCheck.GroundState.Grounded:
                if (crouching)
                {
                    rig.drag = crouchDrag;
                    capsuleCollider.material = crouchPhysicsMaterial;
                    rig.AddRelativeForce(new Vector3(moveInputValue.x, 0, moveInputValue.y) * crouchMoveSpeed, ForceMode.Acceleration);
                }
                else
                {
                    rig.drag = onGroundDrag;
                    capsuleCollider.material = onGroundPhysicsMaterial;
                    rig.AddRelativeForce(new Vector3(moveInputValue.x, 0, moveInputValue.y) * onGroundMoveSpeed, ForceMode.Acceleration);
                }
                break;
            case GroundCheck.GroundState.WallGrounded:
                rig.drag = onWallDrag;
                capsuleCollider.material = onWallPhysicsMaterial;
                rig.AddRelativeForce(new Vector3(moveInputValue.x, 0, moveInputValue.y) * onWallMoveSpeed, ForceMode.Acceleration);
                break;
            case GroundCheck.GroundState.Airborne:
                rig.drag = inAirDrag;
                rig.AddRelativeForce(new Vector3(moveInputValue.x, 0, moveInputValue.y) * inAirMoveSpeed, ForceMode.Acceleration);
                break;
        }
        // Determine the direction of movement
        if (moveInputValue.y > MOVE_THRESHOLD)
        {
            if (moveInputValue.x > MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.ForwardRight;
            }
            else if (moveInputValue.x < -MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.ForwardLeft;
            }
            else
            {
                moveDirection = MoveDirection.Forward;
            }
        }
        else if (moveInputValue.y < -MOVE_THRESHOLD)
        {
            if (moveInputValue.x > MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.BackRight;
            }
            else if (moveInputValue.x < -MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.BackLeft;
            }
            else
            {
                moveDirection = MoveDirection.Back;
            }
        }
        else
        {
            if (moveInputValue.x > MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.Right;
            }
            else if (moveInputValue.x < -MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.Left;
            }
        }

    }

    void StopMoving()
    {
        // Stop looping move anim and transition to idle

        animController.StopWalk();

        moveDirection = MoveDirection.None;
    }
}
