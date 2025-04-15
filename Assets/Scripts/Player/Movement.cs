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

    GroundCheck groundCheck;

    [SerializeField] float moveSpeed = 5f;

    MoveState moveState = MoveState.Idle;
    [HideInInspector] public MoveDirection moveDirection { get; private set; } = MoveDirection.None;

    const float MOVE_THRESHOLD = 0.1f;

    [SerializeField] Transform cameraContainer;
    Vector2 moveInputValue;
    Rigidbody rig;

    void Awake()
    {
        animController = GetComponent<AnimationController>();
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
        // Get Ground Check Reference
        groundCheck = GetComponent<GroundCheck>();
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
        bool curMoving = (moveInputValue.magnitude > MOVE_THRESHOLD);
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
        // Move the player
        rig.AddRelativeForce(new Vector3(moveInputValue.x, 0, moveInputValue.y) * moveSpeed, ForceMode.Acceleration);

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
